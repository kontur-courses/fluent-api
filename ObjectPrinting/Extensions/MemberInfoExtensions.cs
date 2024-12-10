using System.Reflection;

namespace ObjectPrinting.Extensions;

public static class MemberInfoExtensions
{
    public static object? GetValue(this MemberInfo? memberInfo, object obj) =>
        memberInfo switch
        {
            FieldInfo fieldInfo => fieldInfo.GetValue(obj),
            PropertyInfo propertyInfo => propertyInfo.GetValue(obj),
            _ => null
        };

    public static Type? GetMemberType(this MemberInfo memberInfo) =>
        memberInfo.MemberType switch
        {
            MemberTypes.Constructor => memberInfo.DeclaringType,
            MemberTypes.Event => ((EventInfo)memberInfo).EventHandlerType,
            MemberTypes.Field => ((FieldInfo)memberInfo).FieldType,
            MemberTypes.Method => ((MethodInfo)memberInfo).ReturnType,
            MemberTypes.Property => ((PropertyInfo)memberInfo).PropertyType,
            MemberTypes.TypeInfo => ((TypeInfo)memberInfo).BaseType,
            MemberTypes.Custom => memberInfo.DeclaringType,
            MemberTypes.NestedType => memberInfo.DeclaringType,
            MemberTypes.All => memberInfo.DeclaringType,
            _ => throw new ArgumentOutOfRangeException(nameof(memberInfo), memberInfo,
                "Unsupported member type.")
        };
}