using System;
using System.Globalization;

namespace ObjectPrinting.Configs;

public class TypePrintingConfig<TOwner, TProp>
{
    private readonly PrintingContext context;
    private readonly PrintingConfig<TOwner> parent;

    public TypePrintingConfig(PrintingConfig<TOwner> parent, PrintingContext context)
    {
        this.parent = parent;
        this.context = context;
    }

    public PrintingConfig<TOwner> Using(Func<TProp, string> serializer)
    {
        context.typeSerializers[typeof(TProp)] = obj => serializer((TProp)obj);
        return parent;
    }

    public PrintingConfig<TOwner> Using(CultureInfo culture)
    {
        context.typeCultures[typeof(TProp)] = culture;
        return parent;
    }
}