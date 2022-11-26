using System;

namespace ObjectPrinting;

public class MemberPrintingConfig<TMember>
{
    private readonly PrintingConfig<TMember> _printingConfig;

    internal MemberPrintingConfig(PrintingConfig<TMember> printingConfig)
    {
        _printingConfig = printingConfig;
    }

    public PrintingConfig<TMember> Using(Func<TMember, string> stringFunc)
    {
        return _printingConfig;
    }
}