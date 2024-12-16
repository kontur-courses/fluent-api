using System;
using System.Reflection;

namespace ObjectPrinting.Extensions;

public static class MemberInfoExtensions
{
    public static object? GetValue(this MemberInfo memberInfo, object forObject) => 
        memberInfo.MemberType switch
    {
        MemberTypes.Field => ((FieldInfo) memberInfo).GetValue(forObject),
        MemberTypes.Property => ((PropertyInfo) memberInfo).GetValue(forObject),
        _ => throw new ArgumentException()
    };

    public static Type GetMemberType(this MemberInfo memberInfo) => 
        memberInfo.MemberType switch
    {
        MemberTypes.Field => ((FieldInfo) memberInfo).FieldType,
        MemberTypes.Property => ((PropertyInfo) memberInfo).PropertyType,
        _ => throw new ArgumentException()
    };
}