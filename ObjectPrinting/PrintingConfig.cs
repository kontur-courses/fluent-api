using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ObjectPrinting;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private HashSet<Type> excludingTypes = new();
        private HashSet<string> excludingProperties = new();
        private readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
        private readonly Dictionary<string, Func<object, string>> propertiesSerializers = new();


        public PropertyPrintingConfig<TOwner, TProp> ConfigureProperty<TProp>(Expression<Func<TOwner, TProp>> propertySelector)
        {
            var propertyName = ((MemberExpression)propertySelector.Body).Member.Name;
            return new PropertyPrintingConfig<TOwner, TProp>(this, propertyName);
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludingTypes.Add(typeof(T));
            return this;
        }
        
        public PrintingConfig<TOwner> Exclude<TProp>(Expression<Func<TOwner, TProp>> propertySelector)
        {
            var propertyName = ((MemberExpression)propertySelector.Body).Member.Name;
            excludingProperties.Add(propertyName);
            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> ConfigureType<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        internal void AddPropertySerializer<TProp>(string propertyName, Func<TProp, string> serializer)
        {
            propertiesSerializers[propertyName] = obj =>
            {
                var value = (TProp)obj;
                

        
                return serializer(value);
            };
        }

        internal void AddTypeSerializer<TPropType>(Func<TPropType, string> serializer)
        {
            typeSerializers[typeof(TPropType)] = obj => serializer((TPropType)obj);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
            {
                return "null" + Environment.NewLine;
            }

            var objectType = obj.GetType();

            if (typeSerializers.TryGetValue(objectType, out var serializer))
            {
                return serializer(obj) + Environment.NewLine;
            }
                

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(objectType))
            {
                return obj + Environment.NewLine;
            }
                

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(objectType.Name);
            foreach (var propertyInfo in objectType.GetProperties())
            {
                var propertyName = propertyInfo.Name;
                var propertyType = propertyInfo.PropertyType;
        
                if (excludingProperties.Contains(propertyName) || excludingTypes.Contains(propertyType))
                    continue;

                var propertyValue = propertyInfo.GetValue(obj);
                string serializedValue = GetSerializedValue(propertyValue, propertyName, nestingLevel);
                
                sb.Append(identation + propertyName + " = " + serializedValue);
            }

            return sb.ToString();
        }
        
        
        private string GetSerializedValue(object value, string propertyName, int nestingLevel)
        {

            if (propertiesSerializers.TryGetValue(propertyName, out var propertySerializer))
            {
                return propertySerializer(value) + Environment.NewLine;
            }
            
            if (value != null && typeSerializers.TryGetValue(value.GetType(), out var typeSerializer))
            {
                return typeSerializer(value) + Environment.NewLine;
            }


            return PrintToString(value, nestingLevel + 1);
        }
    }

    
    
    

}