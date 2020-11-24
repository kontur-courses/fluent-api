using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class Config
    {
        public readonly List<Type> ExcludedTypes;
        public readonly List<PropertyInfo> ExludedFields;
        public readonly Dictionary<PropertyInfo, Delegate> FieldSerializers;
        public readonly Dictionary<Type, Delegate> TypesSerializer;

        public Config()
        {
            ExcludedTypes = new List<Type>();
            TypesSerializer = new Dictionary<Type, Delegate>();
            ExludedFields = new List<PropertyInfo>();
            FieldSerializers = new Dictionary<PropertyInfo, Delegate>();
        }

        public bool IsExcluded(PropertyInfo propertyInfo)
        {
            return ExludedFields.Contains(propertyInfo) || ExcludedTypes.Contains(propertyInfo.PropertyType);
        }

        public bool IsSpecialSerialize(PropertyInfo propertyInfo, object element, out string result)
        {
            result = "";
            if (!FieldSerializers.ContainsKey(propertyInfo))
                return false;
            result = FieldSerializers[propertyInfo].DynamicInvoke(element)?.ToString();
            return true;
        }

        public bool IsSpecialSerialize(Type type, object element, out string result)
        {
            result = "";
            if (!TypesSerializer.ContainsKey(type))
                return false;
            result = TypesSerializer[type].DynamicInvoke(element)?.ToString();
            return true;
        }
    }
}