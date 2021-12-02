using System;
using System.Reflection;

namespace ObjectPrinting.Extensions
{
    public static class MemberInfoExtensions
    {
        public static object GetValue(this MemberInfo memberInfo, object forObject)
        {
            return memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo) memberInfo).GetValue(forObject),
                MemberTypes.Property => ((PropertyInfo) memberInfo).GetValue(forObject),
                _ => throw new ArgumentException($"Невозможно получить значение для {memberInfo.MemberType}")
            };
        }
        
        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            return memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo) memberInfo).FieldType,
                MemberTypes.Property => ((PropertyInfo) memberInfo).PropertyType,
                _ => throw new ArgumentException($"Невозможно получить тип для {memberInfo.MemberType}")
            };
        }
    }
}