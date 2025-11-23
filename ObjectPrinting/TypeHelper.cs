using System;

namespace ObjectPrinting;

public class TypeHelper
{
    public static bool IsSimpleType(Type type)
    {
        if (type.IsPrimitive) return true;
        if (type == typeof(string)) return true;
        if (type == typeof(decimal)) return true;
        if (type == typeof(DateTime)) return true;
        if (type == typeof(TimeSpan)) return true;
        if (type.IsEnum) return true;

        return false;
    }
}