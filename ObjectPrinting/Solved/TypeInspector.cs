using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting.Solved
{
    public static class TypeInspector
    {
        private static readonly HashSet<Type> FinalTypes = new()
        {
            typeof(string),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(Guid),
        };

        public static bool IsFinalType(Type type) =>
            type.IsPrimitive
            || type.IsEnum
            || FinalTypes.Contains(type);

        public static IEnumerable<MemberAccessor> GetMembers(Type type)
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                if (property.CanRead && property.GetIndexParameters().Length == 0)
                    yield return new MemberAccessor(property);

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                yield return new MemberAccessor(field);
        }
    }
}