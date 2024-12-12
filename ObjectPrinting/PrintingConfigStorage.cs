using System.Collections.Generic;
using System;

public class PrintingConfigStorage
{
    public HashSet<Type> ExcludedTypes { get; } = new();
    public Dictionary<Type, Func<object, string>> TypeSerializationMethods { get; } = new();
    public Dictionary<Type, IFormatProvider> TypeCultures { get; } = new();
    public Dictionary<string, IFormatProvider> PropertyCultures { get; } = new();
    public Dictionary<string, Func<object, string>> PropertySerializationMethods { get; } = new();
    public HashSet<string> ExcludedProperties { get; } = new();
    public Dictionary<string, int> PropertyLengths { get; } = new();
}
