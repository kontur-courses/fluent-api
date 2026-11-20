using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting.Printing.Validation
{
    internal static class MemberAccessor
    {
        private static readonly HashSet<Type> AdditionalSimpleTypes = new()
        {
            typeof(string), typeof(decimal),
            typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan),
            typeof(DateOnly), typeof(TimeOnly),
            typeof(Guid)
        };

        public static bool IsSimpleType(Type type)
            => type.IsPrimitive || type.IsEnum || AdditionalSimpleTypes.Contains(type);

        public static IEnumerable<MemberInfo> GetMembers(Type type)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            var properties = type.GetProperties(flags)
                .Where(p => p.GetGetMethod() != null
                && p.GetIndexParameters().Length == 0);

            return properties.Cast<MemberInfo>().Concat(type.GetFields(flags));
        }

        public static Type GetMemberType(MemberInfo member)
            => member is PropertyInfo property
            ? property.PropertyType
            : ((FieldInfo)member).FieldType;

        public static object? GetValue(MemberInfo member, object obj)
            => member is PropertyInfo property
            ? property.GetValue(obj)
            : ((FieldInfo)member).GetValue(obj);
    }
}