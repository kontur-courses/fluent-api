using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> inner,
            CultureInfo culture)
        {
            return ((IPropertyPrintingConfig<TOwner>)inner).printingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> inner,
            CultureInfo culture)
        {
            return ((IPropertyPrintingConfig<TOwner>)inner).printingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> inner,
            CultureInfo culture)
        {
            return ((IPropertyPrintingConfig<TOwner>)inner).printingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, short> inner,
            CultureInfo culture)
        {
            return ((IPropertyPrintingConfig<TOwner>)inner).printingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, ulong> inner,
            CultureInfo culture)
        {
            return ((IPropertyPrintingConfig<TOwner>)inner).printingConfig;
        }

        public static PrintingConfig<TOwner> Trim<TOwner>(this PropertyPrintingConfig<TOwner, string> inner,
            int size)
        {
            return ((IPropertyPrintingConfig<TOwner>)inner).printingConfig;
        }
    }
}