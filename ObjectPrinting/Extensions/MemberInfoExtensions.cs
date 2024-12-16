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

    public static Type GetMemberType(this MemberInfo? memberInfo) =>
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        memberInfo?.MemberType switch
        {
            MemberTypes.Field => ((FieldInfo)memberInfo).FieldType,
            MemberTypes.Property => ((PropertyInfo)memberInfo).PropertyType,
            _ => throw new ArgumentOutOfRangeException(nameof(memberInfo), memberInfo,
                "Unsupported member type.")
        };
}