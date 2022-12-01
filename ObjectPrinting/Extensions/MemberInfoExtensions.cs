using System;
using System.Reflection;

namespace ObjectPrinting.Extensions
{
    public static class MemberInfoExtensions
    {
        public static object GetValue(this MemberInfo memberInfo, object obj)
        {
            return memberInfo is PropertyInfo
                ? (memberInfo as PropertyInfo).GetValue(obj)
                : memberInfo is FieldInfo
                    ? (memberInfo as FieldInfo).GetValue(obj)
                    : throw new TypeAccessException("");
        }

        public static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                     "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }
    }
}
