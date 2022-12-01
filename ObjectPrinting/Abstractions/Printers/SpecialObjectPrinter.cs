using System;
using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Abstractions.Printers;

public abstract class SpecialObjectPrinter
{
    public abstract bool CanPrint(object obj);

    public string PrintToString(PrintingMemberData memberData, IRootObjectPrinter rootPrinter)
    {
        var obj = memberData.Member;
        if (obj is null)
            throw new ArgumentNullException(nameof(obj));
        if (!CanPrint(obj))
            throw new ArgumentException($"Unable to print {obj}, using printer {GetType().Name}!");
        return InternalPrintToString(memberData, rootPrinter);
    }

    protected abstract string InternalPrintToString(
        PrintingMemberData memberData,
        IRootObjectPrinter rootPrinter);
}