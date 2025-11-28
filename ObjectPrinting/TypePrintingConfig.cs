using System;

namespace ObjectPrinting;

public class TypePrintingConfig<TOwner, TPropType> : ITypePrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> _parentConfig;

    public TypePrintingConfig(PrintingConfig<TOwner> parentConfig)
    {
        _parentConfig = parentConfig;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
    {
        return _parentConfig.WithTypeSerializer(serializer);
    }
}