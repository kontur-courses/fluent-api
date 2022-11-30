namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Crop<TOwner>(this PropertyConfig<TOwner, string> propConfig,
            int maxLen)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
            var crops = ((IPrintingConfig<TOwner>)parentConfig).StringsToCrop;
            var property = ((IPropertyPrintingConfig<TOwner, string>)propConfig).Property;
            crops.Add(property, maxLen);
            return parentConfig;
        }
    }
}