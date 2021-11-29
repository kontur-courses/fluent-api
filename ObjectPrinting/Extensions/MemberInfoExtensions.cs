using System;
using System.Reflection;

namespace ObjectPrinting.Extensions
{
    internal static class MemberInfoExtensions
    {
        public static object GetValue(this MemberInfo memberInfo, object obj)
        {
            if (memberInfo is PropertyInfo propertyInfo)
                return propertyInfo.GetValue(obj);
            if (memberInfo is FieldInfo fieldInfo)
                return fieldInfo.GetValue(obj);
            throw new InvalidOperationException();
        }

        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propertyInfo)
                return propertyInfo.PropertyType;
            if (memberInfo is FieldInfo fieldInfo)
                return fieldInfo.FieldType;
            throw new InvalidOperationException();
        }
    }
}