using System;
using System.Globalization;

namespace ObjectPrinting;

internal class PrintingRule
{
    internal bool IsExcluded { get; set; }
    internal Func<object, string>? Serializer { get; set; } = null;
    internal CultureInfo? Culture { get; set; } = null;
}
    
