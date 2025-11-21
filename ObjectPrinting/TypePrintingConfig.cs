using System;
using System.Globalization;

namespace ObjectPrinting;

public class TypePrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> parentConfig;

    public TypePrintingConfig(PrintingConfig<TOwner> parentConfig)
    {
        this.parentConfig = parentConfig;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
    {
        parentConfig.AddTypeSerializer(serializer);
        return parentConfig;
    }


}