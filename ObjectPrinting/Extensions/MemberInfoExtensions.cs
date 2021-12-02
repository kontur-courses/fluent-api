using System;
using System.Reflection;

namespace ObjectPrinting.Extensions
{
    public static class MemberInfoExtensions
    {
        public static object GetValue(this MemberInfo memberInfo, object obj)
        {
            return memberInfo switch
            {
                PropertyInfo propertyInfo => propertyInfo.GetValue(obj),
                FieldInfo fieldInfo => fieldInfo.GetValue(obj),
                _ => throw new ArgumentOutOfRangeException($"unexpected memberInfo: {memberInfo}")
            };
        }

        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            return memberInfo switch
            {
                PropertyInfo pi => pi.PropertyType,
                FieldInfo fi => fi.FieldType,
                _ => throw new ArgumentOutOfRangeException($"unexpected memberInfo: {memberInfo}")
            };
        }
    }
}