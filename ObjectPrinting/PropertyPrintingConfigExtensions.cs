namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            return propConfig.PropertyInfo == null
                ? SetLengthForType(propConfig, maxLen)
                : SetLengthForProperty(propConfig, maxLen);
        }

        private static PrintingConfig<TOwner> SetLengthForType<TOwner>(
            IPropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            propConfig.ParentConfig.SetMaxLength(typeof(string), maxLen);
            return propConfig.ParentConfig;
        }
        
        private static PrintingConfig<TOwner> SetLengthForProperty<TOwner>(
            PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            propConfig.ParentConfig.SetMaxLength(propConfig.PropertyInfo, maxLen);
            return propConfig.ParentConfig;
        }
    }
}