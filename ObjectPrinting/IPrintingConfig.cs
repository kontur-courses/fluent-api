using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public interface IPrintingConfig
    {
        Dictionary<Type, Func<object, string>> WaysToSerializeTypes { get; }

        Dictionary<Type, CultureInfo> TypesCultures { get; }

        Dictionary<PropertyInfo, Func<object, string>> WaysToSerializeProperties { get; }

        Dictionary<PropertyInfo, int> MaxLengthsOfProperties { get; }
    }
}