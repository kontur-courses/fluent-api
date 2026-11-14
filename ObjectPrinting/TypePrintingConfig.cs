using System;

namespace ObjectPrinting;

public class TypePrintingConfig<TOwner, TPrintingType>
{
    private readonly PrintingConfig<TOwner> _parentConfig;

    public TypePrintingConfig(PrintingConfig<TOwner> parentConfig)
    {
        _parentConfig = parentConfig;
    }

    public PrintingConfig<TOwner> Using(Func<TPrintingType, string> printer)
    {
        ArgumentNullException.ThrowIfNull(printer);
        _parentConfig.SetTypePrinter(typeof(TPrintingType), obj => printer((TPrintingType)obj));
        return _parentConfig;
    }
}