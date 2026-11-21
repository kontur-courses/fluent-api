using System;

namespace ObjectPrinting;

public class ValuePrintingConfig<TOwner, TValue>
{
    private readonly PrintingConfig<TOwner> parentConfig;
    private readonly Action<Func<object, string>> setPrinter;

    internal ValuePrintingConfig(PrintingConfig<TOwner> parentConfig, Action<Func<object, string>> setPrinter)
    {
        this.parentConfig = parentConfig;
        this.setPrinter = setPrinter;
    }

    public PrintingConfig<TOwner> Using(Func<TValue, string> printer)
    {
        ArgumentNullException.ThrowIfNull(printer);
        setPrinter(value => printer((TValue)value));
        return parentConfig;
    }
}