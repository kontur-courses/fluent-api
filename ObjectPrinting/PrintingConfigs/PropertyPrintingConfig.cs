using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting.PrintingConfigs;

public class PropertyPrintingConfig<TOwner, TProp>
{
    protected readonly PrintingConfig<TOwner> Config;
    protected readonly Expression<Func<TOwner, TProp>> Selector;

    public PropertyPrintingConfig(PrintingConfig<TOwner> config, Expression<Func<TOwner, TProp>> selector)
    {
        Config = config;
        Selector = selector;
    }

    public PrintingConfig<TOwner> Using(Func<TProp, string> serializer)
    {
        return Config.SetSerializerFor(Selector, serializer);
    }
}