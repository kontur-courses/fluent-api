using System;


namespace ObjectPrinting
{
    public static class PropertySerializationConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimToLength<TOwner>(this PropertySerializationConfig<TOwner, string> config,
            int maxLength)
        {
            var printingConfig = ((IPropertySerializationConfig<TOwner>) config).PrintingConfig;
            var propertyName = ((IPropertySerializationConfig<TOwner>) config).PropertyName;

            var trimFunc = GetTrimFunc(propertyName, maxLength);

            ((ISettings)printingConfig).Settings.SetPropertyToTrim(propertyName, trimFunc);

            return printingConfig;
        }

        private static Func<string, string> GetTrimFunc(string propertyName, int maxLength)
        {
            return s =>
                propertyName.Substring(0, Math.Min(propertyName.Length, maxLength));
        }
    }
}
