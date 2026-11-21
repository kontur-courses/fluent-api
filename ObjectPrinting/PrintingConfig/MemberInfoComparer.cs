using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting;

internal class MemberInfoComparer : IEqualityComparer<MemberInfo>
{
    public static readonly MemberInfoComparer Instance = new();

    public bool Equals(MemberInfo? left, MemberInfo? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;

        return left.MetadataToken == right.MetadataToken
               && left.Module.Equals(right.Module)
               && left.DeclaringType == right.DeclaringType;
    }

    public int GetHashCode(MemberInfo obj)
    {
        return HashCode.Combine(obj.MetadataToken, obj.Module, obj.DeclaringType);
    }
}