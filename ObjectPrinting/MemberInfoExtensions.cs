using System;
using System.Reflection;

namespace ObjectPrinting;

internal static class MemberInfoExtensions
{
    public static object? GetValue(this MemberInfo memberInfo, object? obj)
    {
        return memberInfo.MemberType switch
        {
            MemberTypes.Field => ((FieldInfo)memberInfo).GetValue(obj),
            MemberTypes.Property => ((PropertyInfo)memberInfo).GetValue(obj),
            _ => throw new ArgumentException($"Member type {memberInfo.MemberType} is not supported.")
        };
    }

    public static Type GetMemberType(this MemberInfo memberInfo)
    {
        return memberInfo.MemberType switch
        {
            MemberTypes.Field => ((FieldInfo)memberInfo).FieldType,
            MemberTypes.Property => ((PropertyInfo)memberInfo).PropertyType,
            _ => throw new ArgumentException($"Member type {memberInfo.MemberType} is not supported.")
        };
    }
}