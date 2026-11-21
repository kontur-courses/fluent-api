using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting;

internal sealed record PrintingConfigState(
    ImmutableHashSet<Type> ExcludedTypes,
    ImmutableHashSet<MemberInfo> ExcludedMembers,
    ImmutableDictionary<Type, Func<object, string>> TypeSerializers,
    ImmutableDictionary<MemberInfo, Func<object, string>> MemberSerializers,
    ImmutableDictionary<Type, CultureInfo> TypeCultures,
    ImmutableDictionary<MemberInfo, CultureInfo> MemberCultures,
    int? StringMaxLength,
    ImmutableDictionary<MemberInfo, int> MemberMaxLengths)
{
    public static readonly PrintingConfigState Empty = new(
        ImmutableHashSet<Type>.Empty,
        ImmutableHashSet.Create<MemberInfo>(MemberInfoComparer.Instance),
        ImmutableDictionary<Type, Func<object, string>>.Empty,
        ImmutableDictionary.Create<MemberInfo, Func<object, string>>(MemberInfoComparer.Instance),
        ImmutableDictionary<Type, CultureInfo>.Empty,
        ImmutableDictionary.Create<MemberInfo, CultureInfo>(MemberInfoComparer.Instance),
        null,
        ImmutableDictionary.Create<MemberInfo, int>(MemberInfoComparer.Instance));
}
