    using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting.Extensions
{
    internal static class TypeExtensions
    {
        public static IEnumerable<MemberInfo> GetFieldsAndProperties(this Type type) =>
            type
                .GetFields()
                .Cast<MemberInfo>()
                .Concat(type.GetProperties());
    }
}