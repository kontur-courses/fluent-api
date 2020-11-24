using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting.Solved
{
    public class Config
    {
        public readonly List<Type> excludedTypes;
        public readonly List<PropertyInfo> exludedFields;
        public readonly Dictionary<PropertyInfo, Delegate> fieldSerializers;
        public readonly Dictionary<Type, Delegate> typesSerializer;

        public Config()
        {
            excludedTypes = new List<Type>();
            typesSerializer = new Dictionary<Type, Delegate>();
            exludedFields = new List<PropertyInfo>();
            fieldSerializers = new Dictionary<PropertyInfo, Delegate>();
        }

        public bool IsExcluded(PropertyInfo propertyInfo)
        {
            return exludedFields.Contains(propertyInfo) || excludedTypes.Contains(propertyInfo.PropertyType);
        }

        public bool IsSpecialSerialize(PropertyInfo propertyInfo, object element, out string result)
        {
            result = "";
            if (!fieldSerializers.ContainsKey(propertyInfo))
                return false;
            result = fieldSerializers[propertyInfo].DynamicInvoke(element)?.ToString();
            return true;
        }

        public bool IsSpecialSerialize(Type type, object element, out string result)
        {
            result = "";
            if (!typesSerializer.ContainsKey(type))
                return false;
            result = typesSerializer[type].DynamicInvoke(element)?.ToString();
            return true;
        }
    }
}