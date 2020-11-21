namespace ObjectPrinting.Solved
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            return propConfig.Using(x => x.Length > maxLen ? x.Substring(0, maxLen) : x);
        }
    }
}