using System;

namespace ObjectPrinting;

public class TypePrintingConfig<TOwner, TPropType> : IInnerPrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> printingConfig;

    public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
    {
        this.printingConfig = printingConfig;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
    {
        printingConfig.AddCustomTypeSerializer(typeof(TPropType), serializer);
        return printingConfig;
    }
}