using System;
using System.Collections;
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
            new Serializer(this).Serialize(obj);

        private readonly Dictionary<Type,Func<object,string>> finalTypes = new Dictionary<Type,Func<object,string>>();
        private readonly Dictionary<PropertyInfo,Func<object,string>> finalProperties = 
            new Dictionary<PropertyInfo,Func<object,string>> ();

        public PrintingConfig(params Type[] finalTypes)
        {
            InitFinalTypes(finalTypes);
        }

        public PrintingConfig()
        {
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            InitFinalTypes(finalTypes);
        }

        private void InitFinalTypes(IEnumerable<Type> finalTypes)
        {
            foreach (var type in finalTypes)
                this.finalTypes[type] = x => x.ToString();
        }
                
        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            finalTypes[typeof(TPropType)] = null;
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner,TPropType>> selector)
        {
            finalProperties[GetPropertyInfo(selector)] = null;
            return this;
        }

        public TypeConfig<TPropType> Serializing<TPropType>()=> 
            new TypeConfig<TPropType>(this);

        public PropertyConfig<TPropType> Serializing<TPropType>(Expression<Func<TOwner,TPropType>> selector)=>
            new PropertyConfig<TPropType>(this,selector);

        //TODO smear it in checks
        private static PropertyInfo GetPropertyInfo<TPropType>(Expression<Func<TOwner, TPropType>> selector) =>
            (PropertyInfo) ((MemberExpression) selector.Body).Member;


        private class Serializer
        {
            private string identation;
            private readonly StringBuilder stringBuilder;
            private readonly PrintingConfig<TOwner> config;
            private readonly Stack<object> serializingObjects;

            public Serializer(PrintingConfig<TOwner> config)
            {
                this.config = config; 
                stringBuilder = new StringBuilder();
                serializingObjects = new Stack<object>();
            }
            
            public string Serialize(TOwner obj)
            {
                SerializeRecursive(obj, 0);
                return stringBuilder.ToString();
            }
    
            private void SerializeRecursive(object obj, int nestingLevel)
            {
                if (serializingObjects.Contains(obj))
                {
                    stringBuilder.AppendLine("this "+obj.GetType().Name);
                    return;
                }
                    
                serializingObjects.Push(obj);
                SerializeByStructure(obj, nestingLevel);
                serializingObjects.Pop();
            }

            private void SerializeByStructure(object obj, int nestingLevel)
            {
                stringBuilder.AppendLine(obj.GetType().Name);
                if (obj.GetType().GetInterfaces().Contains(typeof(IEnumerable)))
                    SerializeAsEnumerable(obj, nestingLevel);
                else                    
                    SerializeAsObject(obj, nestingLevel);
            }

            private void SerializeAsObject(object obj,int nestingLevel)
            {
                foreach (var propertyInfo in obj.GetType().GetProperties())
                {
                    identation = new string('\t', nestingLevel + 1);
                    var value = propertyInfo.GetValue(obj);
                    if (TrySerializeAsNull(propertyInfo, value) ||
                        TrySerializeAsFinal(propertyInfo, value))  
                        continue;
                    stringBuilder.Append(identation + propertyInfo.Name + " = ");
                    SerializeRecursive(value, nestingLevel + 1);
                }
            }

            private void SerializeAsEnumerable(object obj,int nestingLevel)
            {
                foreach (var el in (IEnumerable) obj)
                {
                    identation = new string('\t', nestingLevel + 1);
                    if(TrySerializeAsFinal(el))
                        continue;;
                    stringBuilder.Append(identation);
                    SerializeRecursive(el, nestingLevel + 1);
                }
            }
    
            private void PrintFinalizedProperty(PropertyInfo propertyInfo, string finalized) =>
                stringBuilder.Append(identation + propertyInfo.Name + " = " + finalized + Environment.NewLine);


            private bool TrySerializeAsNull(PropertyInfo propertyInfo, object value)
            {
                if (value != null) return false;
                PrintFinalizedProperty(propertyInfo, "null");
                return true;
            }

            private bool TrySerializeAsFinal(PropertyInfo propertyInfo,object value)
            {
                if (!config.finalProperties.TryGetValue(propertyInfo, out var serializer) &&
                    !config.finalTypes.TryGetValue(propertyInfo.PropertyType, out serializer)) 
                    return false;
                if (serializer != null)
                    PrintFinalizedProperty(propertyInfo,serializer(value));
                return true;
            }

            private bool TrySerializeAsFinal(object obj)
            {
                if (!config.finalTypes.TryGetValue(obj.GetType(), out var serializer)) 
                    return false;
                if(serializer != null)
                    stringBuilder.AppendLine(identation + serializer(obj));
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
                printingConfig.finalTypes[typeof(TPropType)] = 
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
                printingConfig.finalProperties[GetPropertyInfo(selector)] =
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