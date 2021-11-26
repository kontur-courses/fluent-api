using System;
using System.Reflection;

namespace ObjectPrinting.Solved.Extensions
{
    public static class MemberInfoExtensions
    {
        public static object GetValue(this MemberInfo memberInfo, object fromObj)
        {
            return memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)memberInfo).GetValue(fromObj),
                MemberTypes.Property => ((PropertyInfo)memberInfo).GetValue(fromObj),
                _ => throw new NotImplementedException()
            };
        }

        public static bool IsFieldOrProperty(this MemberInfo memberInfo)
        {
            return memberInfo.MemberType == MemberTypes.Field
                   || memberInfo.MemberType == MemberTypes.Property;
        }

        public static Type GetMemberInstanceType(this MemberInfo memberInfo)
        {
            return memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)memberInfo).FieldType,
                MemberTypes.Property => ((PropertyInfo)memberInfo).PropertyType,
                _ => throw new NotImplementedException()
            };
        }
    }
}