using System;

namespace ObjectPrinting;

public class PropertyConfig<TOwner> : Configuration.IPropertyConfig<TOwner>
{
    private readonly PrintingConfig<TOwner> config;
    private readonly string propertyName;

    public PropertyConfig(PrintingConfig<TOwner> config, string propertyName)
    {
        this.config = config;
        this.propertyName = propertyName;
    }

    public PrintingConfig<TOwner> Exclude()
    {
        config.excludedProperties.Add(propertyName);
        return config;
    }

    public PrintingConfig<TOwner> Using(Func<object, string> serializer)
    {
        config.propertySerializers[propertyName] = serializer;
        return config;
    }
 }