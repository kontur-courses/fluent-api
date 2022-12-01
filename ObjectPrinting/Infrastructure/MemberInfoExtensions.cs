using System;
using System.Reflection;

namespace ObjectPrinting.Infrastructure;

public static class MemberInfoExtensions
{
    public static object? GetFieldPropertyValue(this MemberInfo info, object obj) =>
        info switch
        {
            PropertyInfo prop => prop.GetValue(obj),
            FieldInfo field => field.GetValue(obj),
            _ => throw new ArgumentException($"Unable to get value of this member: {info.Name}")
        };
}