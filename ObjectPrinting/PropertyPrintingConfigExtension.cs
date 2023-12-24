namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtension
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int length)
        {
            var printingConfig = ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
            var property = ((IPropertyPrintingConfig<TOwner, string>)propConfig).PropertyInfo;

            ((IPrintingConfig<TOwner>)printingConfig).SerializationSettings.AddPropertyToTrim(property, length);

            return printingConfig;
        }
    }
}