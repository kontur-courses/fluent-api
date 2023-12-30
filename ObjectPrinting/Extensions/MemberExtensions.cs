using System;
using System.Reflection;

namespace ObjectPrinting.Extensions
{
    public static class MemberExtensions
    {
        internal static Type GetMemberType(this MemberInfo memberInfo) =>
            memberInfo.MemberType switch
            {
                MemberTypes.Property => ((PropertyInfo)memberInfo).PropertyType,
                MemberTypes.Field => ((FieldInfo)memberInfo).FieldType,
                _ => throw new NotImplementedException($"Getting type of {memberInfo.MemberType} is not implemented")
            };

        public static object GetValue(this MemberInfo memberInfo, object obj) =>
            memberInfo.MemberType switch
            {
                MemberTypes.Property => ((PropertyInfo)memberInfo).GetValue(obj),
                MemberTypes.Field => ((FieldInfo)memberInfo).GetValue(obj),
                _ => throw new NotImplementedException($"Getting value of {memberInfo.MemberType} is not implemented")
            };
    }
}
