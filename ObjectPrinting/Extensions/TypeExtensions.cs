using System;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting.Extensions
{
    public static class TypeExtensions
    {
        public static MemberInfo[] GetPublicPropertiesAndFields(this Type type)
        {
            return type.GetMembers()
                .Where(m => m is PropertyInfo || m is FieldInfo)
                .ToArray();
        }
    }
}