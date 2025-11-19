using System;
using System.Globalization;

namespace ObjectPrinting;

public class TypeSerializingConfig<TOwner, TType>
{
    private readonly PrintingConfig<TOwner> printingConfig;

    public TypeSerializingConfig(PrintingConfig<TOwner> printingConfig)
    {
        this.printingConfig = printingConfig;
    }
    
    public PrintingConfig<TOwner> Use(Func<TType, string> serializeFunc)
    {
        return printingConfig.AddTypeSerializer(serializeFunc);
    }

    public PrintingConfig<TOwner> Use(CultureInfo cultureInfo)
    {
        return printingConfig.AddTypeCulture<TType>(cultureInfo);
    }
}