using System;
using System.Collections.Generic;
using ObjectPrinting.Abstractions;
using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Implementations;

public class ExcludingRules : IExcludingRules
{
    private readonly HashSet<Type> _excludedTypes = new();

    private readonly HashSet<IReadOnlyList<string>> _excludedMembers = new(new EnumerableEqualityComparer<string>());

    public bool IsExcluded(PrintingMemberData memberData)
    {
        if (memberData.Member is not null && _excludedTypes.Contains(memberData.MemberType))
            return true;
        return _excludedMembers.Contains(memberData.MemberPath);
    }

    public void Exclude<T>() =>
        _excludedTypes.Add(typeof(T));

    public void Exclude(IReadOnlyList<string> memberPath) =>
        _excludedMembers.Add(memberPath);
}