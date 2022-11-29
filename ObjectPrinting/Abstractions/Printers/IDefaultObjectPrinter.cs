using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Abstractions.Printers;

public interface IDefaultObjectPrinter
{
    IExcludingRules ExcludingRules { get; }
    string PrintToString(PrintingMemberData memberData, IRootObjectPrinter rootPrinter);
}