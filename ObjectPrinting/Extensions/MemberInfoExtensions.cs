using System;
using System.Reflection;

namespace ObjectPrinting.Extensions;

public static class MemberInfoExtensions
{
    public static Type GetMemberType(this MemberInfo member)
    {
        return member switch
        {
            PropertyInfo propertyInfo => propertyInfo.PropertyType,
            FieldInfo fieldInfo => fieldInfo.FieldType,
            _ => throw new ArgumentException("member should be property or fieldInfo", nameof(member))
        };
    }
}