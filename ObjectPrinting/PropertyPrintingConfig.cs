using System.Globalization;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> _printingConfig;

    internal PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
    {
        _printingConfig = printingConfig;
    }

    internal PrintingConfig<TOwner> ParentConfig => _printingConfig;

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        return _printingConfig;
    }

    public PrintingConfig<TOwner> Using(CultureInfo culture)
    {
        return _printingConfig;
    }

    public PrintingConfig<TOwner> Using(PrintingConfig<TPropType> propertyPrintingConfig)
    {
        return _printingConfig;
    }
}
