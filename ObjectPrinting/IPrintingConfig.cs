using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting
{
    public interface IPrintingConfig
    {
        Dictionary<string, Delegate> PropertySerializer { get; }
        Dictionary<string, CultureInfo> Cultures { get; }
        Dictionary<string, int> TrimLenghts { get; }
    }
}