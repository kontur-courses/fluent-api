namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            return propConfig.Using(x => x.Length > maxLen && maxLen >= 0 ? x[..maxLen] : x);
        }
    }
}