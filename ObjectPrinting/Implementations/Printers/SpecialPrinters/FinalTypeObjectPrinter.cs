using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ObjectPrinting.Abstractions.Printers;
using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Implementations.Printers.SpecialPrinters;

public class FinalTypeObjectPrinter : SpecialObjectPrinter
{
    private static readonly HashSet<Type> FinalTypes = new()
    {
        typeof(string), typeof(Guid), typeof(DateTime), typeof(TimeSpan), typeof(ITuple)
    };

    public override bool CanPrint(object obj)
    {
        var type = obj.GetType();
        if (type.IsPrimitive)
            return true;
        if (type.IsGenericType)
            type = type.GetGenericTypeDefinition();
        return FinalTypes.Any(final => final.IsAssignableFrom(type));
    }

    protected override string InternalPrintToString(PrintingMemberData memberData, IRootObjectPrinter rootPrinter) =>
        memberData.Member!.ToString()!;
}