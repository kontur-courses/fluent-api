using System;
using System.Collections.Immutable;

namespace ObjectPrinting.Infrastructure;

public record PrintingMemberData(object? Member, ImmutableList<string> MemberPath, ImmutableHashSet<object> Parents)
{
    public PrintingMemberData(object? obj) :
        this(obj, ImmutableList<string>.Empty, ImmutableHashSet<object>.Empty)
    {
    }

    public int Nesting { get; } = MemberPath.Count;

    public Type GetMemberType =>
        Member?.GetType() ?? throw new InvalidOperationException($"{nameof(Member)} is null!");

    public PrintingMemberData CreateForChild(object? child, string childMemberName) =>
        new(child, MemberPath.Add(childMemberName), Parents.Add(Member!));
}