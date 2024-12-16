using System;

namespace ObjectPrinting;

public class TypePrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> printingConfig;

    public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
    {
        if (printingConfig == null)
            throw new ArgumentNullException();

        this.printingConfig = printingConfig;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
    {
        if (serializer == null)
            throw new ArgumentNullException();

        printingConfig.UpdateSerializer(typeof(TPropType), x => serializer((TPropType) x));
        return printingConfig;
    }
}