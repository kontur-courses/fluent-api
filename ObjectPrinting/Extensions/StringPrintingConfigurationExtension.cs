namespace ObjectPrinting.Extensions;

public static class StringPrintingConfigurationExtension
{
    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this PropertyPrintingConfiguration<TOwner, string> propertyStringConfiguration, int length)
    {
        propertyStringConfiguration.ParentConfig
            .AddStringPropertyTrim(propertyStringConfiguration.PropertyMemberInfo.Name, length);

        return propertyStringConfiguration.ParentConfig;
    }
}