using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting.Solved
{
    public class Config
    {
        public List<Type> excludedTypes;
        public List<PropertyInfo> exludedFields;
        public Dictionary<PropertyInfo, Delegate> fieldSerializers;
        public Dictionary<Type, Delegate> typesSerializer;

        public Config(Config config)
        {
            excludedTypes = new List<Type>();
            typesSerializer = new Dictionary<Type, Delegate>();
            exludedFields = new List<PropertyInfo>();
            fieldSerializers = new Dictionary<PropertyInfo, Delegate>();
        }

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

        public Config Exclude(Type type)
        {
            excludedTypes.Add(type);
            return new Config(this);
        }
    }
}