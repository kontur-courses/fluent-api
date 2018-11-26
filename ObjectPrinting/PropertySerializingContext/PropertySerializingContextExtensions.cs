namespace ObjectPrinting
{
    public static class PropertySerializingContextExtensions
    {
        public static PrintingConfig<TOwner> TrimmingToLength<TOwner>(
            this PropertySerializingContext<TOwner, string> context, int length)
        {
            var contextView = (IPropertySerializingContext<TOwner>) context;
            var config = contextView.Config;
            ((IPrintingConfig<TOwner>)config).PropertyTrimmingSettings.Add(contextView.Property, length);
            return config;
        }
    }
}