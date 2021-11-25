using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting.Extensions
{
    internal static class TypeExtensions
    {
        public static IEnumerable<MemberInfo> GetPropertiesAndFields(this Type type)
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            var fields = type.GetFields(bindingFlags);
            var properties = type.GetProperties(bindingFlags);

            return fields.Concat<MemberInfo>(properties);
        }
    }
}