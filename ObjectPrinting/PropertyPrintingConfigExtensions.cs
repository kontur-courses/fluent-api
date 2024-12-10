namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this PropertyPrintingConfig<TOwner, string> propertyPrintingConfig,
        int maxLength)
    {
        return propertyPrintingConfig.ParentConfig;
    }
}
