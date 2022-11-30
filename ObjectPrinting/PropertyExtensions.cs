namespace ObjectPrinting
{
    public static class PropertyExtensions
    {
        public static PrintingConfig<TOwner> CutString<TOwner>(this IPropertyConfig<TOwner, string> config, int maxLength)
        {
            config.Printer.SetStringCut(config.PropertyExpression, maxLength);
            return config.Printer;
        }
    }
}