using System;
using System.Linq.Expressions;

namespace ObjectPrinting;

public record class PropertyPrintingConfig<TOwner, TPropType>
{
    public PrintingConfig<TOwner> PrintingConfig { get; private set; }
    internal Expression<Func<TOwner, TPropType>>? propertySelector;

    public PropertyPrintingConfig(
        PrintingConfig<TOwner> printingConfig,
        Expression<Func<TOwner, TPropType>>? propertySelector = null)
    {
        PrintingConfig = printingConfig;
        this.propertySelector = propertySelector;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        if (propertySelector is null)
            PrintingConfig.config.TypeSerializers[typeof(TPropType)] = print;
        else
            PrintingConfig.config.SetSerializerForPropertyFromExpression(propertySelector, print);
        return PrintingConfig;
    }
}
