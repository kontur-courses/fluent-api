using System.Collections.Generic;
using System.Linq;
using ObjectPrinting.Abstractions;
using ObjectPrinting.Abstractions.Printers;
using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Implementations.Printers;

public class RootObjectPrinter : IRootObjectPrinter
{
    public ICustomPrintersCollector PrintersCollector { get; }
    public IExcludingRules ExcludingRules => _defaultObjectPrinter.ExcludingRules;

    private readonly IDefaultObjectPrinter _defaultObjectPrinter;
    private readonly IFinalTypeObjectPrinter _finalTypeObjectPrinter;
    private readonly IEnumerable<ISpecialObjectPrinter> _specialObjectPrinters;

    public RootObjectPrinter(
        IDefaultObjectPrinter defaultObjectPrinter,
        IFinalTypeObjectPrinter finalTypeObjectPrinter,
        ICustomPrintersCollector customPrintersCollector,
        IEnumerable<ISpecialObjectPrinter> specialObjectPrinters
    )
    {
        _defaultObjectPrinter = defaultObjectPrinter;
        _finalTypeObjectPrinter = finalTypeObjectPrinter;
        PrintersCollector = customPrintersCollector;
        _specialObjectPrinters = specialObjectPrinters;
    }

    public string PrintToString(object? obj) =>
        PrintToString(new PrintingMemberData(obj));

    public string PrintToString(PrintingMemberData memberData)
    {
        var obj = memberData.Member;
        if (obj is null)
            return "null";
        if (memberData.Parents.Contains(obj))
            return "[Loop reference]";

        if (PrintersCollector.TryGetPrinterFor(memberData, out var printer))
            return printer.PrintToString(obj);
        if (_finalTypeObjectPrinter.TryPrintToString(obj, out var result))
            return result;
        var specialPrinter = _specialObjectPrinters.FirstOrDefault(special => special.CanPrint(obj));
        return specialPrinter is not null
            ? specialPrinter.PrintToString(memberData, this)
            : _defaultObjectPrinter.PrintToString(memberData, this);
    }
}