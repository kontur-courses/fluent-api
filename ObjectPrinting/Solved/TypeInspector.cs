using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting.Solved
{
    public static class TypeInspector
    {
        public static bool IsFinalType(Type type) =>
            type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(TimeSpan)
            || type == typeof(Guid);

        public static IEnumerable<MemberAccessor> GetMembers(Type type)
        {
            foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                if (p.CanRead && p.GetIndexParameters().Length == 0)
                    yield return new MemberAccessor(p);

            foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                yield return new MemberAccessor(f);
        }
    }
}