namespace ObjectPrinting.Extensions
{
    public static class PropertyPrintingConfigExtension
    {
        public static PrintingConfig<TOwner> TrimmedStringProperty<TOwner>(
            this PropertyPrintingConfig<TOwner, string> printingConfig, int length)
        {
            return printingConfig.SetSerializer(str => str.Length <= length ? str : str.Substring(0, length));
        }
    }
}