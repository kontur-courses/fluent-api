using System;
using System.Collections.Generic;

namespace ObjectPrinting;

public class PrintingSettings
{
    public HashSet<Type> ExcludedTypes { get; } = [];
    public HashSet<string> ExcludedProperties { get; } = [];
    public Dictionary<Type, Func<object?, string>> TypeSerializers { get; } = new();
    public Dictionary<string, Func<object?, string>> PropertySerializers { get; } = new();
    public Dictionary<Type, IFormatProvider> TypeCultures { get; } = new();
    public Dictionary<string, int> StringTrimLengths { get; } = new();
    public int GlobalStringTrimLength { get; set; } = 0;
}