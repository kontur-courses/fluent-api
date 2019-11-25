namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            var parent = ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
            var property = ((IPropertyPrintingConfig<TOwner, string>) propConfig).ConfigProperty;
            ((IPrintingConfig<TOwner>) parent).TrimLengthDictionary.Add(property, maxLen);

            return ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
        }
    }
}