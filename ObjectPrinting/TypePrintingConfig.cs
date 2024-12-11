using System;

namespace ObjectPrinting;

public class TypePrintingConfig<TOwner, TPropType>(PrintingConfig<TOwner> config)
    : ITypePrintingConfig<TOwner, TPropType>
{
    private PrintingConfig<TOwner> Config { get; } = config;

    public PrintingConfig<TOwner> Using(Func<TPropType, string> func)
    {
        Config.AddTypeSerializer(func);

        return Config;
    }
}