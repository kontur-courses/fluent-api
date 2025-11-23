using System;
using System.Globalization;

namespace ObjectPrinting;

public class TypePrintingConfig<TOwner, TProp>
{
    private readonly PrintingConfig<TOwner> config;

    internal TypePrintingConfig(PrintingConfig<TOwner> config)
    {
        this.config = config;
    }

    public PrintingConfig<TOwner> Using(Func<TProp, string> serializer)
    {
        config.TypeSerializers[typeof(TProp)] = o => serializer((TProp)o);
        return config;
    }

    public PrintingConfig<TOwner> Using(CultureInfo culture)
    {
        if (!typeof(IFormattable).IsAssignableFrom(typeof(TProp)))
            throw new InvalidOperationException("Culture can be applied only to IFormattable");

        config.TypeSerializers[typeof(TProp)] =
            o => ((IFormattable)o).ToString(null, culture);

        return config;
    }
}