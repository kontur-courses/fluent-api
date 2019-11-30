using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting.Core.PrintingConfig
{
    public interface IPrintingConfig
    {
        Dictionary<Type, Func<object, string>> TypePrintingFunctions { get; }
        Dictionary<string, Func<object, string>> PropertyPrintingFunctions { get; }
        Dictionary<Type, CultureInfo> TypeCultures { get; }
    }
}