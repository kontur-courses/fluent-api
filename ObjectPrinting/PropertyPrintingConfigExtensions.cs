using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> printingConfig,
            CultureInfo culture)
            => printingConfig.Using(x => x.ToString(culture));

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> printingConfig,
            CultureInfo culture)
            => printingConfig.Using(x => x.ToString(culture));

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> printingConfig, CultureInfo culture)
            => printingConfig.Using(x => x.ToString(culture));

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> printingConfig, CultureInfo culture)
            => printingConfig.Using(x => x.ToString(culture));

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> printingConfig, int length)
            => printingConfig.Using(x => x.Substring(0, Math.Min(length, x.Length)));
    }
}