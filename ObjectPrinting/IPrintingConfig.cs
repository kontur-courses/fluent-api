using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting
{
    internal interface IPrintingConfig
    {
        Dictionary<Type, Delegate> TypePrintingFunctions { get; }
        Dictionary<string, Delegate> PropertyPrintingFunctions { get; }
        Dictionary<string, int> PropertyStringsLength { get; }
        Dictionary<Type, CultureInfo> TypeCultures { get; }
    }
}