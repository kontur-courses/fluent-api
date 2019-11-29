using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting.Core.PrintingConfig
{
    public interface IPrintingConfig
    {
        Dictionary<Type, Delegate> TypePrintingFunctions { get; }
        Dictionary<string, Delegate> PropertyPrintingFunctions { get; }
        Dictionary<Type, CultureInfo> TypeCultures { get; }
    }
}