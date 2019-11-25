using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting
{
    public interface IPrintingConfig<TOwner>
    {
        Dictionary<Type, Delegate> TypePrintingFunctions { get; }
        Dictionary<string, Delegate> PropertyPrintingFunctions { get; }
        Dictionary<Type, CultureInfo> TypeCultures { get; }
    }
}