using System;
using System.Globalization;

namespace ObjectPrinting;

public class TypePrintingConfig<TOwner, TType> : ITypePrintingConfig<TOwner, TType>
{
    private readonly PrintingConfig<TOwner> printingConfig;

    public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
    {
        this.printingConfig = printingConfig;
    }

    PrintingConfig<TOwner> ITypePrintingConfig<TOwner, TType>.ParentConfig => printingConfig;

    public PrintingConfig<TOwner> Using(Func<TType, string> print)
    {
        var config = (IPrintingConfig<TOwner>)printingConfig;
        config.TypeSerializers[typeof(TType)] = t => print((TType)t);
        return printingConfig;
    }

    public PrintingConfig<TOwner> Using<IFormattable>(CultureInfo culture)
    {
        var config = (IPrintingConfig<TOwner>)printingConfig;
        config.Cultures[typeof(TType)] = culture;
        return printingConfig;
    }
}
