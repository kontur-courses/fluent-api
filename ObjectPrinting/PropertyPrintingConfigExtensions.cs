namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> MaxLength<TOwner>(
        this PropertyPrintingConfig<TOwner, string> propertyPrintingConfig,
        int maxLen)
    {
        var headConfig = propertyPrintingConfig.HeadConfig;
        
        headConfig.PropertiesMaxLength[propertyPrintingConfig.PropertyInfo] = maxLen;
        
        return headConfig;
    }
}