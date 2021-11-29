using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

namespace ObjectPrinting.Extensions
{
    public static class MemberInfoExtensions
    {
        public static object GetMemberValue(this MemberInfo member, object obj)
        {
            return member switch
            {
                PropertyInfo propertyInfo => propertyInfo.GetValue(obj),
                FieldInfo fieldInfo => fieldInfo.GetValue(obj),
                _ => throw new ArgumentException($"{nameof(member)} has no value"),
            };
        }

        public static Type GetMemberType(this MemberInfo member)
        {
            return member switch
            {
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                FieldInfo fieldInfo => fieldInfo.FieldType,
                _ => throw new ArgumentException($"{nameof(member)} has no value"),
            };
        }
    }
}