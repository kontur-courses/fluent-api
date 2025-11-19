using System;

namespace ObjectPrinting;

public class CollectionsConfigurator<TOwner, TCollection>
{
    private readonly PrintingConfig<TOwner> _printingConfig;
    private readonly PrintingConfigData _data;
    private readonly Type _collectionType;

    public CollectionsConfigurator(PrintingConfig<TOwner> printingConfig, PrintingConfigData data, Type collectionType)
    {
        _printingConfig = printingConfig;
        _data = data;
        _collectionType = collectionType;
    }

    public static implicit operator PrintingConfig<TOwner>(CollectionsConfigurator<TOwner, TCollection> configurator)
    {
        return configurator._printingConfig;
    }

    public CollectionsConfigurator<TOwner, TCollection> WithMaxItems(int maxItems)
    {
        _data.CollectionMaxItems[_collectionType] = maxItems;
        return this;
    }

    public CollectionsConfigurator<TOwner, TCollection> WithItemSerialization(Func<object, string> itemSerializer)
    {
        _data.CollectionSerializers[_collectionType] = itemSerializer;
        return this;
    }
    
    public string PrintToString(TOwner obj)
    {
        return _printingConfig.PrintToString(obj);
    }
    
    
}