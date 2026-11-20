using System;
using System.Collections.Generic;
using System.Reflection;
namespace ObjectPrinting.Printing.Validation;
internal class PrintingRules
{
    internal readonly HashSet<MemberKey> ExcludedMembers = new();
    internal readonly HashSet<Type> ExcludedTypes = new();
    internal readonly Dictionary<MemberKey, Func<object, string>> MemberFormatters = new();
    internal readonly Dictionary<Type, Func<object, string>> TypeFormatters = new();


    public PrintDecision ForMember(MemberInfo member, object owner)
    {
        var key = MemberKey.From(member);

        if (ExcludedMembers.Contains(key) || ExcludedTypes.Contains(MemberAccessor.GetMemberType(member)))
            return PrintDecision.Excluded;

        return Resolve(key, MemberAccessor.GetValue(member, owner));
    }

    public PrintDecision ForValue(object? value) => Resolve(null, value);


    private PrintDecision Resolve(MemberKey? key, object? value)
    {
        if (value == null)
            return new PrintDecision(false, null, null);

        var type = value.GetType();

        if (key != null && ExcludedTypes.Contains(type))
            return PrintDecision.Excluded;

        if (key != null && MemberFormatters.TryGetValue(key.Value, out var memberFormatter))
            return new PrintDecision(false, value, memberFormatter);

        if (TypeFormatters.TryGetValue(type, out var typeFormatter))
            return new PrintDecision(false, value, typeFormatter);

        return new PrintDecision(false, value, null);
    }
}