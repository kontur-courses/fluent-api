using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public static class MemberInfoExtensions
{
    public static object? GetMemberValue(this MemberInfo memberInfo, object obj)
    {
        return memberInfo switch
        {
            PropertyInfo property => property.GetValue(obj),
            FieldInfo field => field.GetValue(obj),
            _ => throw new NotSupportedException()
        };
    }
    
    public static Type GetMemberType(this MemberInfo memberInfo)
    {
        return memberInfo switch
        {
            PropertyInfo p => p.PropertyType,
            FieldInfo f => f.FieldType,
            _ => throw new NotSupportedException()
        };
    }
}