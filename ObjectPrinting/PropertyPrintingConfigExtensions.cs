namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Trimm<TOwner>(this PropertyPrintingConfig<string, TOwner> config,
            int lenght)
        {
            return config.Using(x => x[..^lenght]);
        }
    }
}