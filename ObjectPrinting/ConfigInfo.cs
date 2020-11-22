using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting
{
    public class ConfigInfo
    {
        private ImmutableHashSet<Type> excludedTypes;
        private ImmutableDictionary<Type, Delegate> typeToSerialization;
        private ImmutableHashSet<PropertyInfo> excludedProperties;
        private ImmutableDictionary<PropertyInfo, Delegate> propertyToSerialization;

        public ConfigInfo()
        {
            excludedTypes = ImmutableHashSet<Type>.Empty;
            typeToSerialization = ImmutableDictionary<Type, Delegate>.Empty;
            excludedProperties = ImmutableHashSet<PropertyInfo>.Empty;
            propertyToSerialization = ImmutableDictionary<PropertyInfo, Delegate>.Empty;
        }
        
        public ConfigInfo(ConfigInfo configInfo)
        {
            excludedTypes = configInfo.excludedTypes;
            typeToSerialization = configInfo.typeToSerialization;
            excludedProperties = configInfo.excludedProperties;
            propertyToSerialization = configInfo.propertyToSerialization;
        }

        public ConfigInfo AddToExcluding(Type type)
        {
            return new ConfigInfo(this) {excludedTypes = excludedTypes.Add(type)};
        }
        
        public ConfigInfo AddToExcluding(PropertyInfo property)
        {
            return new ConfigInfo(this) {excludedProperties = excludedProperties.Add(property)};
        }
        
        public ConfigInfo SetCustomSerialization<TProperty>(Func<TProperty, string> serialization)
        {
            var newTypeToSerialization = typeToSerialization
                .Add(typeof(TProperty), serialization);
            return new ConfigInfo(this) {typeToSerialization = newTypeToSerialization};
        }
        
        public ConfigInfo SetCustomSerialization<TProperty>(Func<TProperty, string> serialization,
            PropertyInfo property)
        {
            var newPropertyToSerialization = propertyToSerialization
                .Add(property, serialization);
            return new ConfigInfo(this) {propertyToSerialization = newPropertyToSerialization};
        }

        public bool ShouldToExclude(PropertyInfo property)
        {
            return excludedTypes.Contains(property.PropertyType) 
                   || excludedProperties.Contains(property);
        }

        public Delegate GetSpecialSerialization(PropertyInfo property)
        {
            if (propertyToSerialization.ContainsKey(property))
                return propertyToSerialization[property];
            if (typeToSerialization.ContainsKey(property.PropertyType))
                return typeToSerialization[property.PropertyType];
            return null;
        }
    }
}