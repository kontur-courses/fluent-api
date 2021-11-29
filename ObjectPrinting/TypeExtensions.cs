using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public static class TypeExtensions
    {
        public static IEnumerable<MemberInfo> GetPropertiesAndFields(this Type type)
        {
            var result = new List<MemberInfo>();
            result.AddRange(type.GetProperties(BindingFlags.Instance | BindingFlags.Public));
            result.AddRange(type.GetFields(BindingFlags.Instance | BindingFlags.Public));

            return result.ToArray();
        }

        public static object GetMemberValue(this MemberInfo member, object obj)
        {
            return member.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)member).GetValue(obj),
                MemberTypes.Property => ((PropertyInfo)member).GetValue(obj),
                _ => throw new ArgumentException(
                    "Input MemberInfo must be if type FieldInfo or PropertyInfo")
            };
        }

        public static Type GetMemberType(this MemberInfo member)
        {
            return member.MemberType switch
            {
                MemberTypes.Event => ((EventInfo)member).EventHandlerType,
                MemberTypes.Field => ((FieldInfo)member).FieldType,
                MemberTypes.Method => ((MethodInfo)member).ReturnType,
                MemberTypes.Property => ((PropertyInfo)member).PropertyType,
                _ => throw new ArgumentException(
                    "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo")
            };
        }
    }
}