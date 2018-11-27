namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> pc, int length)
        {
            pc.Using(str => str.Length <= length ? str : str.Substring(length));
            return pc.PrintingConfig;
        }
    }
}