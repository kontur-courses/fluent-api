using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    interface IPrintingConfig
    {
        Dictionary<Type, Delegate> TypePrintingMethod { get; }
        Dictionary<PropertyInfo, Delegate> PropertyPrintingMethod { get; }
        Dictionary<Type, CultureInfo> CultureForNumber { get; }
    }
}