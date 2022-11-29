namespace ObjectPrinting.Solved;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this PropertyPrintingConfig<TOwner, string> propertyConfig, int maxLength)
    {
        propertyConfig.AlternativePrint = str => str[..maxLength];
        return ((IPropertyPrintingConfig<TOwner, string>)propertyConfig).ParentConfig;
    }
}