using System;

namespace ObjectPrinting;

public class StringPropertyConfig<TOwner> : Configuration.IStringPropertyConfig<TOwner>
{
    private readonly PrintingConfig<TOwner> config;
    private readonly string propertyName;

    public StringPropertyConfig(PrintingConfig<TOwner> config, string propertyName)
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

    public PrintingConfig<TOwner> TrimToLength(int maxLength)
    {
        if (maxLength < 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Length cannot be negative");

        config.trimLengths[propertyName] = maxLength;
        return config;
    }
}