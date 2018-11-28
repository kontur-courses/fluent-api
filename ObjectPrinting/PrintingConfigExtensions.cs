using System.Globalization;
// ReSharper disable UnusedMember.Global

namespace ObjectPrinting
{
    public static class PrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, int> printingConfigContainer,
            CultureInfo currentCulture)
        {
            var printingConfig = ((IPrintingConfigContainer<TOwner>) printingConfigContainer).PrintingConfig;
            ((IPrintingConfig<TOwner>) printingConfig).SetCultureFor<int>(currentCulture);
            return printingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, double> printingConfigContainer,
            CultureInfo currentCulture)
        {
            var printingConfig = ((IPrintingConfigContainer<TOwner>)printingConfigContainer).PrintingConfig;
            ((IPrintingConfig<TOwner>)printingConfig).SetCultureFor<double>(currentCulture);
            return printingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, float> printingConfigContainer,
            CultureInfo currentCulture)
        {
            var printingConfig = ((IPrintingConfigContainer<TOwner>)printingConfigContainer).PrintingConfig;
            ((IPrintingConfig<TOwner>)printingConfig).SetCultureFor<float>(currentCulture);
            return printingConfig;
        }

        public static PrintingConfig<TOwner> TrimToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> printingConfigContainer,
            int length)
        {
            var printingConfig = ((IPrintingConfigContainer<TOwner>) printingConfigContainer).PrintingConfig;
            var member = ((IMemberConfig) printingConfigContainer).GetMember;
            ((IPrintingConfig<TOwner>) printingConfig).SetTrimmingFor(member, length);
            return printingConfig;
        }
    }
}
