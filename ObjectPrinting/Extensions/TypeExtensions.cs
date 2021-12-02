using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<MemberInfo> GetSerializedMembers(this Type type, Predicate<MemberInfo> selector)
        {
            return type.GetMembers()
                .Where(mi => mi.MemberType == MemberTypes.Field || mi.MemberType == MemberTypes.Property)
                .Where(mi => selector(mi));
        }
    }
}