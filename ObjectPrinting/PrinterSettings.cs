using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting;

public class PrinterSettings
{
    public int MaxRecursionDepth { get; }
    public IReadOnlySet<Type> ExcludedTypes { get; }
    public IReadOnlySet<MemberInfo> ExcludedMembers { get; }
    public IReadOnlyDictionary<Type, Func<object, string>> TypeSerializers { get; }
    public IReadOnlyDictionary<MemberInfo, Func<object, string>> MemberSerializers { get; }
    public IReadOnlyDictionary<Type, CultureInfo> TypeCultures { get; }
    public IReadOnlyDictionary<MemberInfo, int> TrimLengths { get; }

    public PrinterSettings(
        int maxRecursionDepth,
        IReadOnlySet<Type> excludedTypes,
        IReadOnlySet<MemberInfo> excludedMembers,
        IReadOnlyDictionary<Type, Func<object, string>> typeSerializers,
        IReadOnlyDictionary<MemberInfo, Func<object, string>> memberSerializers,
        IReadOnlyDictionary<Type, CultureInfo> typeCultures,
        IReadOnlyDictionary<MemberInfo, int> trimLengths)
    {
        MaxRecursionDepth = maxRecursionDepth;
        ExcludedTypes = excludedTypes;
        ExcludedMembers = excludedMembers;
        TypeSerializers = typeSerializers;
        MemberSerializers = memberSerializers;
        TypeCultures = typeCultures;
        TrimLengths = trimLengths;
    }
}