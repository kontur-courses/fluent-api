namespace ObjectPrinting.Solved
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
            => propConfig.Using(s => s.Length > maxLen ? s.Substring(0, maxLen) : s);
    }
}