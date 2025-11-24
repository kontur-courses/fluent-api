using System;

namespace ObjectPrinting.PrintingConfigs;

public class TypePrintingConfig<TOwner, TProp>
{
    internal readonly PrintingConfig<TOwner> Config;
    
    internal TypePrintingConfig(PrintingConfig<TOwner> config)
    {
        Config = config;
    }

    public PrintingConfig<TOwner> Using(Func<TProp, string> serializer)
    {
        return Config.SetSerializerFor(serializer);
    }
}