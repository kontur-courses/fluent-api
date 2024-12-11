using System;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TPropType, TOwner>(PrintingConfig<TOwner> config, string propertyNameName)
    : IPropertyPrintingConfig<TPropType, TOwner>
{
    private PrintingConfig<TOwner> Config { get; } = config;
    private string PropertyName { get; } = propertyNameName;

    public PrintingConfig<TOwner> Using(Func<TPropType, string> func)
    {
        Config.AddPropertySerializer(PropertyName, func);

        return Config;
    }
}