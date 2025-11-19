using System;
using System.Globalization;

namespace ObjectPrinting;

public class PropertyConfigurator<TOwner, TProp>
{
    private readonly PrintingConfig<TOwner> _printingConfig;
    private readonly PrintingConfigData _data;
    private readonly string _propertyName;

    public PropertyConfigurator(PrintingConfig<TOwner> printingConfig, PrintingConfigData data, string propertyName)
    {
        _printingConfig = printingConfig;
        _data = data;
        _propertyName = propertyName;
    }

    public PrintingConfig<TOwner> WithSerialization(Func<TProp, string> serializer)
    {
        _data.PropertySerializers[_propertyName] = obj => serializer((TProp)obj);
        return _printingConfig;
    }

    internal PropertyConfigurator<TOwner, TProp> WithInternalCulture(CultureInfo culture)
    {
        _data.PropertyCultures[_propertyName] = culture;
        return this;
    }
}