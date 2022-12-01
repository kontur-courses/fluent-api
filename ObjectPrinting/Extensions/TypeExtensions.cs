using System;
using System.Collections.Generic;

namespace ObjectPrinting.Extensions
{
    public static class TypeExtensions
    {
        private static readonly HashSet<Type> notPrimitivesFinalTypes = new HashSet<Type>()
        {
            typeof(Guid),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(decimal),
            typeof(string),
        };

        public static bool IsFinal(this Type type)
        {
            return type.IsPrimitive
                || notPrimitivesFinalTypes.Contains(type)
                || type.IsEnum
                || type.IsFinalNullable();
        }

        private static bool IsFinalNullable(this Type type)
        {
            return type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                && type.GetGenericArguments()[0].IsFinal();
        }
    }
}
