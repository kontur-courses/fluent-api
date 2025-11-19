using System;

namespace ObjectPrinting;

public class TypeConfigurator<TOwner, T>
{
    private readonly PrintingConfig<TOwner> _printingConfig;
    private readonly PrintingConfigData _data;

    public TypeConfigurator(PrintingConfig<TOwner> printingConfig, PrintingConfigData data)
    {
        _printingConfig = printingConfig;
        _data = data;
    }

    public PrintingConfig<TOwner> WithSerialization(Func<T, string> serializer)
    {
        _data.TypeSerializers[typeof(T)] = obj => serializer((T)obj);
        return _printingConfig;
    }
}