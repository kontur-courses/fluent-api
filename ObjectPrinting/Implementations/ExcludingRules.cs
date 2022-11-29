using System;
using System.Collections.Generic;
using ObjectPrinting.Abstractions;
using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Implementations;

public class ExcludingRules : IExcludingRules
{
    private readonly HashSet<Type> _excludedTypes = new();

    private readonly HashSet<IReadOnlyList<string>> _excludedMembers = new(new EnumerableEqualityComparer<string>());

    public bool IsExcluded(PrintingMemberData memberData) =>
        _excludedTypes.Contains(memberData.GetMemberType) || _excludedMembers.Contains(memberData.MemberPath);

    public void Exclude<T>() =>
        Exclude(typeof(T));

    public void Exclude(Type type) =>
        _excludedTypes.Add(type);

    public void Exclude(IReadOnlyList<string> memberPath) =>
        _excludedMembers.Add(memberPath);
}