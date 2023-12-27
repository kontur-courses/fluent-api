using ObjectPrinting.Configs;

namespace ObjectPrinting.Extensions
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this TypePrintingConfig<TOwner, string> typeConfig, int length)
        {
            return typeConfig.Using(s => s.Truncate(length));
        }
    }
}