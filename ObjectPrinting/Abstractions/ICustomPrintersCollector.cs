using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ObjectPrinting.Abstractions.Printers;
using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Abstractions;

public interface ICustomPrintersCollector
{
    void AddPrinterFor<T>(ICustomPrinterProvider printerProvider);
    void AddPrinterFor(IReadOnlyList<string> memberPath, ICustomPrinterProvider printerProvider);
    bool TryGetPrinterFor(PrintingMemberData memberData, [NotNullWhen(true)] out ICustomObjectPrinter? printer);
}