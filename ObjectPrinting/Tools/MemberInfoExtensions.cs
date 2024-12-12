using System;
using System.Reflection;

namespace ObjectPrinting.Tools;

public static class MemberInfoExtensions
{
    public static string TryGetFullName(this MemberInfo memberInfo)
        => $"{TryGetType(memberInfo).FullName}.{memberInfo.Name}";

    public static Type TryGetType(this MemberInfo memberInfo)
    {
        if (!IsPropertyOrField(memberInfo))
            throw new ArgumentException("Provided member must be Field or Property");

        return memberInfo.MemberType == MemberTypes.Field
            ? ((FieldInfo)memberInfo).FieldType
            : ((PropertyInfo)memberInfo).PropertyType;
    }
    
    public static bool IsPropertyOrField(this MemberInfo memberInfo)
        => memberInfo.MemberType is MemberTypes.Field or MemberTypes.Property;
}