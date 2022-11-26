using System;
using System.Reflection;

namespace ObjectPrinting.Infrastructure;

public static class MemberInfoExtensions
{
    public static Type GetFieldPropertyType(this MemberInfo info) =>
        info switch
        {
            PropertyInfo prop => prop.PropertyType,
            FieldInfo field => field.FieldType,
            _ => throw new ArgumentException($"Unable to get type of this member: {info.Name}")
        };

    public static object? GetFieldPropertyValue(this MemberInfo info, object obj) =>
        info switch
        {
            PropertyInfo prop => prop.GetValue(obj),
            FieldInfo field => field.GetValue(obj),
            _ => throw new ArgumentException($"Unable to get value of this member: {info.Name}")
        };
}