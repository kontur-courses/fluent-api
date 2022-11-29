using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Abstractions.Printers;

public interface IRootObjectPrinter
{
    ICustomPrintersCollector PrintersCollector { get; }
    IExcludingRules ExcludingRules { get; }

    string PrintToString(object? obj);
    string PrintToString(PrintingMemberData memberData);
}