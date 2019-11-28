using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class SerializationInfo
    {
        private readonly Dictionary<Type, CultureInfo> cultureRules;
        private readonly HashSet<string> excludedProperties;
        private readonly HashSet<Type> excludedTypes;
        private readonly Dictionary<string, Func<object, string>> propertyRules;
        private readonly Dictionary<string, Func<string, string>> trimRules;
        private readonly Dictionary<Type, Func<object, string>> typeRules;

        public SerializationInfo()
        {
            propertyRules = new Dictionary<string, Func<object, string>>();
            excludedProperties = new HashSet<string>();
            excludedTypes = new HashSet<Type>();
            cultureRules = new Dictionary<Type, CultureInfo>();
            typeRules = new Dictionary<Type, Func<object, string>>();
            trimRules = new Dictionary<string, Func<string, string>>();
        }


        public void ExcludeType(Type type)
        {
            excludedTypes.Add(type);
        }

        public void ExcludeProperty(PropertyInfo property)
        {
            excludedProperties.Add(property.Name);
        }

        public void AddTypeRule(Type type, Func<object, string> serialization)
        {
            typeRules[type] = serialization;
        }

        public void AddPropertyRule(PropertyInfo property, Func<object, string> serialization)
        {
            propertyRules[property.Name] = serialization;
        }

        public void AddCultureRule(Type type, CultureInfo serialization)
        {
            cultureRules[type] = serialization;
        }

        public void AddTrimRule(PropertyInfo propInfo, int length)
        {
            trimRules[propInfo.Name] = x =>
            {
                var str = x;
                return str.Substring(0, length > str.Length ? str.Length : length);
            };
        }

        public bool Excluded(PropertyInfo propertyInfo)
        {
            return excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperties.Contains(propertyInfo.Name);
        }

        public bool TryGetSerialization(PropertyInfo propertyInfo, out StringBuilder stringBuilder, object obj)
        {
            stringBuilder = new StringBuilder();

            if (propertyRules.TryGetValue(propertyInfo.Name, out var propertyRule))
            {
                stringBuilder.Append(propertyRule(propertyInfo.GetValue(obj)));
                return true;
            }

            if (typeRules.TryGetValue(propertyInfo.PropertyType, out var typeRule))
            {
                stringBuilder.Append(typeRule(propertyInfo.GetValue(obj)));
                return true;
            }

            if (trimRules.TryGetValue(propertyInfo.Name, out var trimRule))
            {
                stringBuilder.Append(trimRule(propertyInfo.GetValue(obj).ToString()));
                return true;
            }

            if (cultureRules.TryGetValue(propertyInfo.PropertyType, out var cultureInfo))
            {
                stringBuilder.Append(((double) propertyInfo.GetValue(obj)).ToString(cultureInfo));
                return true;
            }

            return false;
        }
    }
    //TODO Add serialization for IEnumerable
}