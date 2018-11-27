using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private readonly Dictionary<Type,Func<object,string>> specialTypes = new Dictionary<Type,Func<object,string>>();
        private readonly Dictionary<PropertyInfo,Func<object,string>> specialProperties = 
            new Dictionary<PropertyInfo,Func<object,string>> ();
        
        private static readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        
        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var value = propertyInfo.GetValue(obj);
                if (TryFinalizeProperty(propertyInfo, value, out var serialized))
                {
                    if (serialized != null) 
                        sb.Append(identation + propertyInfo.Name + " = " + serialized + Environment.NewLine);
                }
                else sb.Append(identation + propertyInfo.Name + " = " + PrintToString(value, nestingLevel + 1));
            }
            return sb.ToString();
        }

        private bool TryFinalizeProperty(PropertyInfo propertyInfo,object value ,out string serialized)
        {
            serialized = null;
            //var value = propertyInfo.GetValue(obj);

            if (value == null)
                serialized = "null";
            else if (specialProperties.TryGetValue(propertyInfo, out var serializer) ||
                     specialTypes.TryGetValue(propertyInfo.PropertyType, out serializer))
            {
                if (serializer != null)
                    serialized = serializer(value);
                return true;
            }
            else if (finalTypes.Contains(propertyInfo.PropertyType))
                serialized = value.ToString();

            return serialized != null;
        }
        
        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            specialTypes[typeof(TPropType)] = null;
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner,TPropType>> selector)
        {
            specialProperties[GetPropertyInfo(selector)] = null;
            return this;
        }

        public TypeConfig<TPropType> Serializing<TPropType>()=> 
            new TypeConfig<TPropType>(this);

        public PropertyConfig<TPropType> Serializing<TPropType>(Expression<Func<TOwner,TPropType>> selector)=>
            new PropertyConfig<TPropType>(this,selector);

        //TODO smear checks
        private static PropertyInfo GetPropertyInfo<TPropType>(Expression<Func<TOwner, TPropType>> selector) =>
            (PropertyInfo) ((MemberExpression) selector.Body).Member;
    
        
        public class TypeConfig<TPropType> : IPrintingConfig<TOwner>
        {
            private readonly PrintingConfig<TOwner> printingConfig;
            public TypeConfig(PrintingConfig<TOwner> printingConfig)
            {
                this.printingConfig = printingConfig;
            }

            public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
            {
                printingConfig.specialTypes[typeof(TPropType)] = 
                    obj => serializer((TPropType)obj);
                return printingConfig;
            }

            PrintingConfig<TOwner> IPrintingConfig<TOwner>.PrintingConfig => printingConfig;
        }
        
    
        public class PropertyConfig<TPropType> : IPrintingConfig<TOwner>
        {
            private readonly PrintingConfig<TOwner> printingConfig;
            private readonly Expression<Func<TOwner, TPropType>> selector;

            public PropertyConfig(PrintingConfig<TOwner> printingConfig, Expression<Func<TOwner,TPropType>>  selector)
            {
                this.printingConfig = printingConfig;
                this.selector = selector;
            }

            public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
            {
                printingConfig.specialProperties[GetPropertyInfo(selector)] =
                    obj => serializer((TPropType)obj);
                return printingConfig;
            }
            
            PrintingConfig<TOwner> IPrintingConfig<TOwner>.PrintingConfig => printingConfig;
        }
    }
    
    public interface IPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
    }

    public static class PrintingConfigExtenstions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PrintingConfig<TOwner>.TypeConfig<int> config,
            CultureInfo currentCulture) => config.Using(i => i.ToString(currentCulture));

        public static PrintingConfig<TOwner> TrimToLength<TOwner>(
            this PrintingConfig<TOwner>.PropertyConfig<string> printingConfig,
            int length) => printingConfig.Using(x => x.Substring(0, length));
    }
}