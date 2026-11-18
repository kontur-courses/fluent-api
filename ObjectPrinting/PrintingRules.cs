using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting;

internal sealed class PrintingRules
{
    internal ImmutableHashSet<Type> ExcludedTypes { get; }
    internal ImmutableHashSet<MemberPath> ExcludedMembers { get; }

    internal ImmutableDictionary<Type, Func<object, string>> TypeSerializationRules { get; }
    internal ImmutableDictionary<MemberPath, Func<object, string>> MemberSerializationRules { get; }

    internal ImmutableDictionary<Type, CultureInfo> TypeCultureRules { get; }
    internal ImmutableDictionary<MemberPath, CultureInfo> MemberCultureRules { get; }

    internal ImmutableDictionary<Type, int> TypeTruncateRules { get; }
    internal ImmutableDictionary<MemberPath, int> MemberTruncateRules { get; }

    internal PrintingRules()
    {
        ExcludedTypes = [];
        ExcludedMembers = [];
        TypeSerializationRules = ImmutableDictionary.Create<Type, Func<object, string>>();
        MemberSerializationRules = ImmutableDictionary.Create<MemberPath, Func<object, string>>();
        TypeCultureRules = ImmutableDictionary.Create<Type, CultureInfo>();
        MemberCultureRules = ImmutableDictionary.Create<MemberPath, CultureInfo>();
        TypeTruncateRules = ImmutableDictionary.Create<Type, int>();
        MemberTruncateRules = ImmutableDictionary.Create<MemberPath, int>();
    }

    private PrintingRules(
        PrintingRules source,
        ImmutableHashSet<Type>? excludedTypes = null,
        ImmutableHashSet<MemberPath>? excludedMembers = null,
        ImmutableDictionary<Type, Func<object, string>>? typeSerializationRules = null,
        ImmutableDictionary<MemberPath, Func<object, string>>? memberSerializationRules = null,
        ImmutableDictionary<Type, CultureInfo>? typeCultureRules = null,
        ImmutableDictionary<MemberPath, CultureInfo>? memberCultureRules = null,
        ImmutableDictionary<Type, int>? typeTruncateRules = null,
        ImmutableDictionary<MemberPath, int>? memberTruncateRules = null)

    {
        ExcludedTypes = excludedTypes ?? source.ExcludedTypes;
        ExcludedMembers = excludedMembers ?? source.ExcludedMembers;
        TypeSerializationRules = typeSerializationRules ?? source.TypeSerializationRules;
        MemberSerializationRules = memberSerializationRules ?? source.MemberSerializationRules;
        TypeCultureRules = typeCultureRules ?? source.TypeCultureRules;
        MemberCultureRules = memberCultureRules ?? source.MemberCultureRules;
        TypeTruncateRules = typeTruncateRules ?? source.TypeTruncateRules;
        MemberTruncateRules = memberTruncateRules ?? source.MemberTruncateRules;
    }

    internal PrintingRules ExcludeType(Type type) =>
        new(this, excludedTypes: ExcludedTypes.Add(type));

    internal PrintingRules ExcludeMember(MemberPath member) =>
        new(this, excludedMembers: ExcludedMembers.Add(member));

    internal PrintingRules AddTypeSerializationRule<TType>(Func<TType, string> serializationRule) =>
        new(this, typeSerializationRules: TypeSerializationRules.SetItem(typeof(TType), obj => serializationRule((TType)obj)));

    internal PrintingRules AddMemberSerializationRule<TMember>(MemberPath member, Func<TMember, string> serializationRule) =>
        new(this, memberSerializationRules: MemberSerializationRules.SetItem(member, obj => serializationRule((TMember)obj)));

    internal PrintingRules AddTypeCultureRule(Type type, CultureInfo culture) =>
        new(this, typeCultureRules: TypeCultureRules.SetItem(type, culture));

    internal PrintingRules AddMemberCultureRule(MemberPath member, CultureInfo culture) =>
        new(this, memberCultureRules: MemberCultureRules.SetItem(member, culture));

    internal PrintingRules AddTypeTruncateRule(Type type, int length) =>
        new(this, typeTruncateRules: TypeTruncateRules.SetItem(type, length));

    internal PrintingRules AddMemberTruncateRule(MemberPath member, int length) =>
        new(this, memberTruncateRules: MemberTruncateRules.SetItem(member, length));
}