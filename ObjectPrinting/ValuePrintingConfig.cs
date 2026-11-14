using System;

namespace ObjectPrinting;

public class ValuePrintingConfig<TOwner, TValue>
{
    private readonly PrintingConfig<TOwner> _parentConfig;
    private readonly Action<Func<object, string>> _setPrinter;

    internal ValuePrintingConfig(PrintingConfig<TOwner> parentConfig, Action<Func<object, string>> setPrinter)
    {
        _parentConfig = parentConfig;
        _setPrinter = setPrinter;
    }

    public PrintingConfig<TOwner> Using(Func<TValue, string> printer)
    {
        ArgumentNullException.ThrowIfNull(printer);
        _setPrinter(value => printer((TValue)value));
        return _parentConfig;
    }
}