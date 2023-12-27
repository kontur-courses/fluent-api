using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static IEnumerable<PropertyInfo> IgnoreTypes(this IEnumerable<PropertyInfo> properties, IEnumerable<Type> ignoredTypes)
        {
            return properties
                .Where(p => !ignoredTypes.Contains(p.PropertyType));
        }
        
        public static IEnumerable<PropertyInfo> IgnoreProperties(this IEnumerable<PropertyInfo> properties, IEnumerable<PropertyInfo> ignoredProperties)
        {
            return properties
                .Except(ignoredProperties);
        }
    }
}