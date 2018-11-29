using System;
using System.Globalization;

// ReSharper disable UnusedMember.Global

namespace ObjectPrinting
{
    public static class PrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TType>(
            this TypePrintingConfig<TOwner, TType> printingConfigContainer,
            CultureInfo currentCulture) where TType : IFormattable
        {
            var printingConfig = ((IPrintingConfigContainer<TOwner>) printingConfigContainer).PrintingConfig;
            ((IPrintingConfig<TOwner>) printingConfig).SetCultureFor<TType>(currentCulture);
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
