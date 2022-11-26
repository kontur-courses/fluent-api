using System;

namespace ObjectPrinting;

public class MemberPrintingConfig<TOwner, TMember> : IMemberPrintingConfig<TOwner>
{
    private readonly PrintingConfig<TOwner> _printingConfig;
    private Func<TMember, string> _printFunction = null!;

    public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig)
    {
        _printingConfig = printingConfig;
    }

    public PrintingConfig<TOwner> Using(Func<TMember, string> printFunc)
    {
        _printFunction = printFunc;
        return _printingConfig;
    }

    PrintingConfig<TOwner> IMemberPrintingConfig<TOwner>.PrintingConfig => _printingConfig;

    string IMemberPrintingConfig<TOwner>.Print(object obj)
    {
        if (obj is not TMember member)
            throw new ArgumentException($"{nameof(obj)} should be of type {typeof(TMember)}");
        return _printFunction(member);
    }
}