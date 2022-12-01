using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class Configurations
    {
        public IList<Type> ExcludedTypes { get; }
        public IList<PropertyInfo> ExcludedProperties { get; }
        public IDictionary<Type, CultureInfo> Cultures { get; }
        public IDictionary<Type, Delegate> SerializationOfTypes { get; }
        public IDictionary<PropertyInfo, Delegate> SerializationOfProperties { get; }

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
