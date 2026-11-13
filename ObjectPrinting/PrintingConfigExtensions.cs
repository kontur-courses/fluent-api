using System;
using System.Collections.Generic;

namespace ObjectPrinting;

public static class PrintingConfigExtensions
{
    public static PrintingConfig<TOwner> Exclude<TOwner>(this PrintingConfig<TOwner> config, Type excludedType)
    {
        ArgumentNullException.ThrowIfNull(excludedType);
        config.ExcludeType(excludedType);
        return config;
    }

    public static PrintingConfig<TOwner> Exclude<TOwner>(
        this PrintingConfig<TOwner> config,
        IEnumerable<Type> excludedTypes)
    {
        ArgumentNullException.ThrowIfNull(excludedTypes);
        foreach (var excludedType in excludedTypes)
        {
            config.Exclude(excludedType);
        }

        return config;
    }
}