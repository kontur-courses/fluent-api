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
        public string PrintToString(TOwner obj)=>
            new Printer(this).PrintRecursive(obj);

        private readonly Dictionary<Type,Func<object,string>> specialTypes = new Dictionary<Type,Func<object,string>>();
        private readonly Dictionary<PropertyInfo,Func<object,string>> specialProperties = 
            new Dictionary<PropertyInfo,Func<object,string>> ();
        
        private static readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        
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


        private class Printer
        {
            private string identation;
            private readonly StringBuilder stringBuilder;
            private readonly PrintingConfig<TOwner> config;

            public Printer(PrintingConfig<TOwner> config)
            {
                this.config = config; 
                stringBuilder = new StringBuilder();
            }
            
            public string PrintRecursive(TOwner obj)
            {
                PrintRecursive(obj, 0);
                return stringBuilder.ToString();
            }
    
            private void PrintRecursive(object obj, int nestingLevel)
            {
                var type = obj.GetType();
                stringBuilder.AppendLine(type.Name);
                foreach (var propertyInfo in type.GetProperties())
                {
                    identation = new string('\t', nestingLevel + 1);
                    var value = propertyInfo.GetValue(obj);
                    if (TryNullSerialize(propertyInfo, value) ||
                        TrySpecialSerialize(propertyInfo, value) ||
                        TryFinalSerialize(propertyInfo, value))  
                        continue;
                    stringBuilder.Append(identation + propertyInfo.Name + " = ");
                    PrintRecursive(value, nestingLevel + 1);
                }
            }
    
            private void PrintFinalizedProperty(PropertyInfo propertyInfo, string finalized) =>
                stringBuilder.Append(identation + propertyInfo.Name + " = " + finalized + Environment.NewLine);


            private bool TryNullSerialize(PropertyInfo propertyInfo, object value)
            {
                if (value != null) return false;
                PrintFinalizedProperty(propertyInfo, "null");
                return true;
            }

            private bool TrySpecialSerialize(PropertyInfo propertyInfo,object value)
            {
                if (!config.specialProperties.TryGetValue(propertyInfo, out var serializer) &&
                    !config.specialTypes.TryGetValue(propertyInfo.PropertyType, out serializer)) 
                    return false;
                if (serializer != null)
                    PrintFinalizedProperty(propertyInfo,serializer(value));
                return true;
            }
    
            private bool TryFinalSerialize(PropertyInfo propertyInfo, object value)
            {
                if (!finalTypes.Contains(propertyInfo.PropertyType)) 
                    return false;
                PrintFinalizedProperty(propertyInfo, value.ToString());
                return true;
            }
        }
        
        
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