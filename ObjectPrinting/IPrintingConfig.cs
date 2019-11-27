using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    interface IPrintingConfig
    {
        Dictionary<Type, Func<object, string>> CustomTypesPrints { get; }
        Dictionary<PropertyInfo, Func<object, string>> CustomPropertiesPrints { get; }
        Dictionary<Type, CultureInfo> CultureInfo { get; }
    }
}
