using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using ObjectPrinting.Abstractions.Printers;

namespace ObjectPrinting.Implementations.Printers;

public class FinalTypeObjectPrinter : IFinalTypeObjectPrinter
{
    private static readonly HashSet<Type> FinalTypes = new()
    {
        typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(ITuple)
    };

    public bool IsFinalType(Type type)
    {
        if (type.IsPrimitive)
            return true;
        if (type.IsGenericType)
            type = type.GetGenericTypeDefinition();
        return FinalTypes.Any(final => final.IsAssignableFrom(type));
    }

    public bool TryPrintToString(object obj, [NotNullWhen(true)] out string? result)
    {
        result = default;

        if (!IsFinalType(obj.GetType()))
            return false;

        result = obj.ToString()!;
        return true;
    }
}