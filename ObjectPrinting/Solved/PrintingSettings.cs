using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.Solved;

public class PrintingSettings
{
    public HashSet<Type> ExcludedTypes = [];
    public HashSet<MemberInfo> ExcludedMembers = [];
    public Dictionary<Type, Func<object, string>> TypeSerializers = new();
    public Dictionary<MemberInfo, Func<object, string?>> MemberSerializers = new();
    public Dictionary<Type, CultureInfo> TypeCultures = new();
    public Dictionary<MemberInfo, CultureInfo> MemberCultures = new();

    public Type[] PrimitiveTypes =
    [
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan), typeof(decimal), typeof(Guid)
    ];
}