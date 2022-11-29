using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Abstractions.Printers;

public interface ISpecialObjectPrinter
{
    bool CanPrint(object obj);
    string PrintToString(PrintingMemberData memberData, IRootObjectPrinter rootObjectPrinter);
}