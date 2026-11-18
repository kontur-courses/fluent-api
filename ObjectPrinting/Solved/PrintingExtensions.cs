using ObjectPrinting.Solved.Interfaces;

namespace ObjectPrinting.Solved
{
    public static class PrintingExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this ITypePrinting<TOwner, string> config, int maxLen)
            => config.Using(s => s.Length > maxLen ? s.Substring(0, maxLen) : s);

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this IPropertyPrinting<TOwner, string> config, int maxLen)
            => config.Using(s => s.Length > maxLen ? s.Substring(0, maxLen) : s);
    }
}