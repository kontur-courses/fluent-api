using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting.Extensions
{
    internal static class TypeExtensions
    {
        public static IEnumerable<MemberInfo> GetSerializedMembers(this Type type)
            => type.GetMembers()
                .Where(m => m.IsSerializedMemberType());
    }
}