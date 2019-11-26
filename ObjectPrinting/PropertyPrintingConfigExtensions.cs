namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            var configInterface = propConfig as IPropertyPrintingConfig<TOwner, string>;
            var parentInterface = configInterface.ParentConfig as IPrintingConfig<TOwner>;
            var property = ((IPropertyPrintingConfig<TOwner, string>) propConfig).ConfigProperty;

            if (parentInterface.LengthsOfStringProperties.ContainsKey(property))
                parentInterface.LengthsOfStringProperties[property] = maxLen;
            else
                parentInterface.LengthsOfStringProperties.Add(property, maxLen);

            return configInterface.ParentConfig;
        }
    }
}