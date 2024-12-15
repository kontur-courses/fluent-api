namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this PropertyPrintingConfig<TOwner, string> propertyPrintingConfig,
        int maxLength)
    {
        var propertyPrintingConfigInterface = (IPropertyPrintingConfig<TOwner>)propertyPrintingConfig;
        var printOverride = propertyPrintingConfigInterface.PrintOverride;

        Func<string, string> newPrintOverride = printOverride == null
            ? str => str[..maxLength]
            : str => printOverride(str)[..maxLength];

        return propertyPrintingConfig.Using(newPrintOverride);
    }
}
