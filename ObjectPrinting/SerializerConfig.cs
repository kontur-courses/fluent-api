using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System;

namespace ObjectPrinting
{
    public class SerializerConfig
    {
        public HashSet<object> serializedObjects;
        public HashSet<Type> excludedTypes;
        public HashSet<PropertyInfo> excludedProperties;
        public Dictionary<Type, Delegate> typeSerialization;
        public Dictionary<PropertyInfo, Delegate> propertiesSerialization;

        public SerializerConfig() 
        {
            serializedObjects = new HashSet<object>();
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<PropertyInfo>();
            typeSerialization = new Dictionary<Type, Delegate>();
            propertiesSerialization = new Dictionary<PropertyInfo, Delegate>();

        }

        public SerializerConfig(SerializerConfig configuration)
        {
            serializedObjects = new HashSet<object>(configuration.serializedObjects);
            excludedTypes = new HashSet<Type>(configuration.excludedTypes);
            excludedProperties = new HashSet<PropertyInfo>(configuration.excludedProperties);
            typeSerialization = new Dictionary<Type, Delegate>(configuration.typeSerialization);
            propertiesSerialization = new Dictionary<PropertyInfo, Delegate>(configuration.propertiesSerialization);
        }

    }
}