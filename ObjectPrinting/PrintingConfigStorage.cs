using System.Collections.Generic;
using System;
using System.Reflection;

public class PrintingConfigStorage
{
    public HashSet<Type> ExcludedTypes { get; } = new();
    public Dictionary<Type, Func<object, string>> TypeSerializationMethods { get; } = new();
    public Dictionary<Type, IFormatProvider> TypeCultures { get; } = new();
    public Dictionary<PropertyInfo, IFormatProvider> PropertyCultures { get; } = new();
    public Dictionary<PropertyInfo, Func<object, string>> PropertySerializationMethods { get; } = new();
    public HashSet<PropertyInfo> ExcludedProperties { get; } = new();
    public Dictionary<PropertyInfo, int> PropertyLengths { get; } = new();
}
