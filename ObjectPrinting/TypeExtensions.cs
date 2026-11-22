using System;

namespace ObjectPrinting;

public static class TypeExtensions
{
    public static bool IsSimpleType(this Type type)
    {
        return type.IsPrimitive
               || type.IsEnum
               || type == typeof(string)
               || type == typeof(decimal)
               || type == typeof(Guid)
               || type == typeof(DateTime)
               || type == typeof(DateTimeOffset)
               || type == typeof(TimeSpan)
               || type == typeof(DateOnly)
               || type == typeof(TimeOnly);
    }
}