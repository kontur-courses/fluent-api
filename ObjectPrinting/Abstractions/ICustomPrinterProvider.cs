using System.Diagnostics.CodeAnalysis;
using ObjectPrinting.Abstractions.Printers;

namespace ObjectPrinting.Abstractions;

public interface ICustomPrinterProvider
{
    bool TryGetPrinter([NotNullWhen(true)] out ICustomObjectPrinter? printer);
}