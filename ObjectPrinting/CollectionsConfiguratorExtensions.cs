using System;
using System.Collections.Generic;

namespace ObjectPrinting;

public static class CollectionsConfiguratorExtensions
{
    public static CollectionsConfigurator<TOwner, TCollection> WithItemSerialization<TOwner, TCollection, TItem>(
        this CollectionsConfigurator<TOwner, TCollection> configurator,
        Func<TItem, string> itemSerializer)
    {
        return configurator.WithItemSerialization(obj => 
        {
            if (obj is TItem item)
                return itemSerializer(item);
            return obj?.ToString() ?? "null";
        });
    }

    public static CollectionsConfigurator<TOwner, TCollection> Trim<TOwner, TCollection>(
        this CollectionsConfigurator<TOwner, TCollection> configurator,
        int start, int end)
    {
        return configurator.WithItemSerialization(str =>
        {
            var s = str as string;
            if (s == null) return "null";
            
            var length = Math.Min(s.Length - start, end - start);
            return length > 0 ? s.Substring(start, length) : string.Empty;
        });
    }
}