using System.Globalization;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
{
    private readonly PrintingConfig<TOwner> _printingConfig;
    private CultureInfo? _cultureInfo;
    private Func<object, string>? _printOverride;
    private bool _isExcluded;

    internal PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
    {
        _printingConfig = printingConfig;
    }

    PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => _printingConfig;

    bool IPropertyPrintingConfig<TOwner>.IsExcluded => _isExcluded;

    CultureInfo? IPropertyPrintingConfig<TOwner>.CultureInfo => _cultureInfo;

    Func<object, string>? IPropertyPrintingConfig<TOwner>.PrintOverride => _printOverride;

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        ArgumentNullException.ThrowIfNull(print);

        _printOverride = obj => print((TPropType)obj);
        return _printingConfig;
    }

    public PrintingConfig<TOwner> Using(CultureInfo cultureInfo)
    {
        _cultureInfo = cultureInfo;
        return _printingConfig;
    }

    internal PrintingConfig<TOwner> Exclude()
    {
        _isExcluded = true;
        return _printingConfig;
    }
}
