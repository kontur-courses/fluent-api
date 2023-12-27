using ObjectPrinting.Configs;

namespace ObjectPrinting.Extensions
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propertyConfig, int length)
        {
            return propertyConfig.Using(s => s.Truncate(length));
        }
    }
}