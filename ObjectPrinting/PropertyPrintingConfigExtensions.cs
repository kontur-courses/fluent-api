using System;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TRoot> TrimmedToLength<TRoot>(
            this PropertyPrintingConfig<TRoot, string> propertyConfig,
            int maxLength)
        {
            ArgumentNullException.ThrowIfNull(propertyConfig);

            return propertyConfig.SetMaxLength(maxLength);
        }
    }
}