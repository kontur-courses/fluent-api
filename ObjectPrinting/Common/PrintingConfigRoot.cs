using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.Common
{
    internal class PrintingConfigRoot
    {
        internal HashSet<Type> ExcludedTypes { get; set; } = new HashSet<Type>();
        internal Dictionary<Type, Func<object, string>> TypeSerializers { get; set; } = new Dictionary<Type, Func<object, string>>();
        internal Dictionary<Type, CultureInfo> NumericTypeCulture { get; set; } = new Dictionary<Type, CultureInfo>();
        internal Dictionary<PropertyInfo, Func<object, string>> PropertySerializers { get; } = new Dictionary<PropertyInfo, Func<object, string>>();
        internal HashSet<PropertyInfo> ExcludedProperties { get; } = new HashSet<PropertyInfo>();
        internal Dictionary<PropertyInfo, int> MaxStringPropertyLengths { get; } = new Dictionary<PropertyInfo, int>();
    }
}