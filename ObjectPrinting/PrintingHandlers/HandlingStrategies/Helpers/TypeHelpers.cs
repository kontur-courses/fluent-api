using System;

namespace ObjectPrinting.PrintingHandlers.HandlingStrategies.Helpers;

internal static class TypeHelpers
{
    public static bool IsTypePrimitive(this Type? type)
    {
        if (type == null) return false;
        return type.IsPrimitive || type == typeof(decimal) || type == typeof(Guid);
    }
}