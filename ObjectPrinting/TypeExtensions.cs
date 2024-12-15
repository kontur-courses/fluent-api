using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting
{
    public static class TypeExtensions
    {
        public static IEnumerable<MemberInfo> GetPublicFieldsAndProperties(this Type type)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance;
            return type.GetProperties(flags)
                .Cast<MemberInfo>()
                .Concat(type.GetFields(flags))
                .ToArray();
        }
    }
}
