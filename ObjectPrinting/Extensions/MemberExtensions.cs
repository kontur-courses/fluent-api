using System;
using System.Reflection;

namespace ObjectPrinting.Extensions
{
    internal static class MemberExtensions
    {
        public static object GetValue(this MemberInfo memberInfo, object obj)
        {
            return memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)memberInfo).GetValue(obj),
                MemberTypes.Property => ((PropertyInfo)memberInfo).GetValue(obj),
                _ => throw new NotImplementedException($"Значение для {memberInfo.MemberType} не определено")
            };
        }

        internal static Type GetMemberType(this MemberInfo memberInfo)
        {
            return memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)memberInfo).FieldType,
                MemberTypes.Property => ((PropertyInfo)memberInfo).PropertyType,
                _ => throw new NotImplementedException($"Значение для {memberInfo.MemberType} не определено")
            };
        }
    }
}