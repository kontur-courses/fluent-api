using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ObjectPrinting.Abstractions.Printers;
using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Implementations.Printers.SpecialPrinters;

public class FinalTypeObjectPrinter : ISpecialObjectPrinter
{
    private static readonly HashSet<Type> FinalTypes = new()
    {
        typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(ITuple)
    };

    public bool CanPrint(object obj)
    {
        var type = obj.GetType();
        if (type.IsPrimitive)
            return true;
        if (type.IsGenericType)
            type = type.GetGenericTypeDefinition();
        return FinalTypes.Any(final => final.IsAssignableFrom(type));
    }

    public string PrintToString(PrintingMemberData memberData, IRootObjectPrinter rootPrinter)
    {
        var obj = memberData.Member;
        if (obj is null || !CanPrint(obj))
            throw new ArgumentException("Unable to print this member, using this printer!");

        return obj.ToString()!;
    }
}