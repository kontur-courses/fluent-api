using System;
using System.Reflection;

namespace ObjectPrinting.Extensions
{
    internal static class MemberInfoExtensions
    {
        public static bool IsSerializedMemberType(this MemberInfo member)
            => member.MemberType == MemberTypes.Field ||
               member.MemberType == MemberTypes.Property;

        public static Type GetTypeOfPropertyOrField(this MemberInfo member)
        {
            return member switch
            {
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                FieldInfo fieldInfo => fieldInfo.FieldType,
                _ => throw new ArgumentException("You can GetType only from Fields and Properties")
            };
        }

        public static object GetValue(this MemberInfo member, object obj)
        {
            return member switch
            {
                PropertyInfo propertyInfo => propertyInfo.GetValue(obj),
                FieldInfo fieldInfo => fieldInfo.GetValue(obj),
                _ => throw new ArgumentException("You can GetValue only from Fields and Properties")
            };
        }
    }
}