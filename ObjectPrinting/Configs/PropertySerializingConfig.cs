using System;

namespace ObjectPrinting;

public class PropertySerializingConfig<TOwner, TProp>
{
    protected readonly PrintingConfig<TOwner> printingConfig;
    protected readonly string propertyName;

    public PropertySerializingConfig(PrintingConfig<TOwner> printingConfig, string propertyName)
    {
        this.printingConfig = printingConfig;
        this.propertyName = propertyName;
    }

    public PrintingConfig<TOwner> Use(Func<TProp, string> serializer)
    {
        return printingConfig.AddPropertySerializer(propertyName, serializer);
    }
}