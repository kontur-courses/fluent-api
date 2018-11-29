using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting.Solved
{
    public interface IPrintingConfigurationHolder
    {
        HashSet<Type> excludedTypes { get; }
        HashSet<string> excludedProperties { get; }
        Dictionary<Type, Delegate> typeSerilizers { get; }
        Dictionary<string, Delegate> propertySerializers { get; }
        Dictionary<Type, CultureInfo> cultureInfos { get; }
        Dictionary<string, int> trimedParams { get; }
    }
}