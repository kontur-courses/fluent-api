using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class Configurations
    {
        public List<Type> ExcludedTypes { get; }
        public List<PropertyInfo> ExcludedProperties { get; }
        public Dictionary<Type, CultureInfo> Cultures { get; }
        public Dictionary<Type, Delegate> SerializationOfTypes { get; }
        public Dictionary<PropertyInfo, Delegate> SerializationOfProperties { get; }

        public Configurations()
        {
            ExcludedTypes = new List<Type>();
            ExcludedProperties = new List<PropertyInfo>();
            Cultures = new Dictionary<Type, CultureInfo>();
            SerializationOfTypes = new Dictionary<Type, Delegate>();
            SerializationOfProperties = new Dictionary<PropertyInfo, Delegate>();
        }
    }
}
