using System;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>
{
    public PrintingConfig<TOwner> OriginalConfig { get; }
    public MemberInfo? PropertyName { get; }

    public PropertyPrintingConfig(PrintingConfig<TOwner> config)
    {
        ArgumentNullException.ThrowIfNull(config);

        OriginalConfig = config;
    }

    public PropertyPrintingConfig(PrintingConfig<TOwner> config, MemberInfo propertyName)
    {
        ArgumentNullException.ThrowIfNull(config);

        OriginalConfig = config;
        PropertyName = propertyName;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> serializeFunc)
    {
        if (string.IsNullOrEmpty(PropertyName?.Name))
        {
            OriginalConfig.AddTypeSerializer(serializeFunc);
        }
        else
        {
            OriginalConfig.AddPropertySerializer(PropertyName.Name, serializeFunc);
        }

        return OriginalConfig;
    }

    public PrintingConfig<TOwner> TrimmedToLength(int length)
    {
        if (PropertyName is not null)
        {
            OriginalConfig.AddStringPropertyTrim(PropertyName.Name, length);
        }

        return OriginalConfig;
    }
}