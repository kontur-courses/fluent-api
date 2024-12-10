namespace ObjectPrinting.Extensions;

public static class StringPrintingConfigurationExtension
{
    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this PrintingConfiguration<TOwner, string> propertyStringConfiguration, int length)
    {
        propertyStringConfiguration.ParentConfig
            .AddStringPropertyTrim(propertyStringConfiguration.PropertyMemberInfo.Name, length);

        return propertyStringConfiguration.ParentConfig;
    }
}