using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting
{
    public static class TypeExtensions
    {
        public static IEnumerable<MemberInfo> GetFieldsAndProperties(this Type type)
        {
            var fields = type.GetFields();
            var properties = type.GetProperties();

            return fields.Concat<MemberInfo>(properties);
        }
    }
}