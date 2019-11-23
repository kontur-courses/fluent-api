using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NUnit.Framework.Constraints;
using ObjectPrinting.Configs.ConfigInterfaces;

namespace ObjectPrinting.Configs
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<PropertyInfo> excludedProperties;
        private readonly Dictionary<Type, Func<Type, string>> alternativeSerializations;

        HashSet<Type> IPrintingConfig<TOwner>.ExcludedTypes => excludedTypes;
        Dictionary<Type, Func<Type, string>> IPrintingConfig<TOwner>.AlternativeSerializations => alternativeSerializations;

        public PrintingConfig()
        {
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<PropertyInfo>();
            alternativeSerializations = new Dictionary<Type, Func<Type, string>>();
        }
        
        public PrintingConfig(
            HashSet<Type> excludedTypes,
            HashSet<PropertyInfo> excludedProperties,
            Dictionary<Type, Func<Type, string>> alternativeSerializations)
        {
            this.excludedTypes = new HashSet<Type>(excludedTypes);
            this.excludedProperties = new HashSet<PropertyInfo>(excludedProperties);
            this.alternativeSerializations = new Dictionary<Type, Func<Type, string>>(alternativeSerializations);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (excludedTypes.Contains(obj.GetType()))
                return string.Empty;
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if(excludedTypes.Contains(propertyInfo.PropertyType)) continue;
                if(excludedProperties.Contains(propertyInfo)) continue;
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            var newExcludingTypes = new HashSet<Type>(excludedTypes) {typeof(T)};
            return new PrintingConfig<TOwner>(newExcludingTypes, excludedProperties, alternativeSerializations);
        }
        
        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> propertyDelegate)
        {
            var propertyInfo = ((MemberExpression) propertyDelegate.Body).Member as PropertyInfo;
            if(propertyInfo == null) throw new ArgumentException();
            var newExcludingProperties = new HashSet<PropertyInfo>(excludedProperties) {propertyInfo};
            return new PrintingConfig<TOwner>(excludedTypes, newExcludingProperties, alternativeSerializations);
        }

        public PropertySerializationConfig<TOwner, TPropType> Serialize<TPropType>()
        {
            return new PropertySerializationConfig<TOwner, TPropType>(this);
        }
        
        public PropertySerializationConfig<TOwner, TPropType> Serialize<TPropType>(Func<TOwner, TPropType> propertyDelegate)
        {
            return new PropertySerializationConfig<TOwner, TPropType>(this);
        }
    }
}