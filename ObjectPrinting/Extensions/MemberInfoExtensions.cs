using System;
using System.Reflection;

namespace ObjectPrinting.Extensions
{
    internal static class MemberInfoExtensions
    {
        public static object GetValue(this MemberInfo memberInfo, object obj) =>
            memberInfo switch
            {
                PropertyInfo propertyInfo => propertyInfo.GetValue(obj),
                FieldInfo fieldInfo => fieldInfo.GetValue(obj),
                _ => throw new ArgumentException($"Unexpected memberInfo: {memberInfo}")
            };
        
        public static Type GetMemberType(this MemberInfo memberInfo) =>
            memberInfo switch
            {
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                FieldInfo fieldInfo => fieldInfo.FieldType,
                _ => throw new ArgumentException($"Unexpected memberInfo: {memberInfo}")
            };
    }
}