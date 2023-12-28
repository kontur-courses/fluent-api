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

    }
}