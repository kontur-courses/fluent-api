using System;
using System.Collections.Immutable;
using System.Reflection;
using ObjectPrinting.PrintingMembers;

namespace ObjectPrinting
{
    public record PrintingConfig
    {
        public ImmutableHashSet<Type> ExcludingTypes { get; init; } = ImmutableHashSet<Type>.Empty;

        public ImmutableHashSet<MemberInfo> ExcludingMembers { get; init; } = ImmutableHashSet<MemberInfo>.Empty;

        public ImmutableDictionary<Type, Func<object, string>> TypePrinting { get; init; } =
            ImmutableDictionary<Type, Func<object, string>>.Empty;

        public ImmutableDictionary<MemberInfo, Func<MemberInfo, string>> MemberPrinting { get; init; } =
            ImmutableDictionary<MemberInfo, Func<MemberInfo, string>>.Empty;

        public IPrintingMemberFactory PrintingMemberFactory { get; init; } = new DefaultPrintingMemberFactory();
    }
}