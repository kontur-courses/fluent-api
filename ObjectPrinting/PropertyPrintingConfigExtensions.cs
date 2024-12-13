namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> TrimmedTo<TOwner>(
        this IPropertyPrintingConfig<string?, TOwner> config,
        int length)
    {
        return config.Using(str => str == null ? "null" : str[..length]);
    }
}