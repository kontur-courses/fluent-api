using System;
using System.Diagnostics.CodeAnalysis;
using ObjectPrinting.Abstractions;
using ObjectPrinting.Abstractions.Configs;
using ObjectPrinting.Abstractions.Printers;
using ObjectPrinting.Implementations.Printers;

namespace ObjectPrinting.Implementations.Configs;

public class MemberPrintingConfig<TOwner, TMember> : IMemberPrintingConfig<TOwner, TMember>, ICustomPrinterProvider
{
    private readonly PrintingConfig<TOwner> _printingConfig;
    private Func<TMember, string>? _printFunction;

    public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig)
    {
        _printingConfig = printingConfig;
    }

    public PrintingConfig<TOwner> Using(Func<TMember, string> printFunc)
    {
        _printFunction = printFunc;
        return _printingConfig;
    }

    bool ICustomPrinterProvider.TryGetPrinter([NotNullWhen(true)] out ICustomObjectPrinter? printer)
    {
        printer = default;
        if (_printFunction is null)
            return false;
        printer = new CustomObjectPrinter<TMember>(_printFunction);
        return true;
    }
}