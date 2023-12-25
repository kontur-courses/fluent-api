using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System;

namespace ObjectPrinting
{
    public class Config
    {
        public HashSet<object> serializedObjects;
        public HashSet<Type> excludedTypes;
        public HashSet<PropertyInfo> excludedProperties;
        public Dictionary<Type, CultureInfo> cultures;
        public Dictionary<Type, Delegate> typeSerialization;
        public Dictionary<PropertyInfo, Delegate> propertiesSerialization;

        public Config() 
        {
            serializedObjects = new HashSet<object>();
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<PropertyInfo>();
            cultures = new Dictionary<Type, CultureInfo>();
            typeSerialization = new Dictionary<Type, Delegate>();
            propertiesSerialization = new Dictionary<PropertyInfo, Delegate>();

        }

    }
}