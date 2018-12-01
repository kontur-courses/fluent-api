using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public interface IPrintingConfigurationHolder
    {
        HashSet<Type> ExcludedTypes { get; }
        HashSet<PropertyInfo> ExcludedProperties { get; }
        Dictionary<Type, Func<object, string>> TypeSerilizers { get; }
        Dictionary<PropertyInfo, Func<object, string>> PropertySerializers { get; }
        Dictionary<Type, CultureInfo> CultureInfos { get; }
        Dictionary<string, int> TrimedParams { get; }
    }
}