using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Abstractions.Printers;

public interface IRootObjectPrinter
{
    public string LineSplitter { get; }
    ICustomPrintersCollector PrintersCollector { get; }
    IExcludingRules ExcludingRules { get; }

    string PrintToString(object? obj);
    string PrintToString(PrintingMemberData memberData);
}