using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting;

public class PrintingConfigData
{
    public HashSet<Type> ExcludedTypes { get; } = new();
    public HashSet<string> ExcludedProperties { get; } = new();
    public Dictionary<Type, Func<object, string>> TypeSerializers { get; } = new();
    public Dictionary<Type, CultureInfo> TypeCultures { get; } = new();
    public Dictionary<string, Func<object, string>> PropertySerializers { get; } = new();
    public Dictionary<string, CultureInfo> PropertyCultures { get; } = new();
    public Dictionary<Type, Func<object, string>> CollectionSerializers { get; } = new();
    public Dictionary<Type, int> CollectionMaxItems { get; } = new();
}