using System;
using System.Reflection;

namespace ObjectPrinting.Extensions
{
    public static class MemberInfoExtensions
    {
        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    return propertyInfo.PropertyType;
                case FieldInfo fieldInfo:
                    return fieldInfo.FieldType;
                default:
                    throw new ArgumentException("Member is not a property or a field");
            }
        }

        public static object GetMemberValue(this MemberInfo memberInfo, object obj)
        {
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    return propertyInfo.GetValue(obj);
                case FieldInfo fieldInfo:
                    return fieldInfo.GetValue(obj);
                default:
                    throw new ArgumentException("Member is not a property or a field");
            }
        }
    }
}