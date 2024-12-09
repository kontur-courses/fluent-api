using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.Solved;

public class DataConfig
{
    public readonly HashSet<Type> ExcludedTypes = new();
    public readonly HashSet<MemberInfo> ExcludedProperties = new();

    public readonly Dictionary<Type, Func<object, string>> TypeSerializers = new();
    public readonly Dictionary<MemberInfo, Func<object, string>> PropertySerializers = new();
    public readonly Dictionary<Type, CultureInfo> TypeCultures = new();
    public readonly Dictionary<MemberInfo, int> PropertyTrim = new();

}