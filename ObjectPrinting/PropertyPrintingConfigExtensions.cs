using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        #region Using culture all numeric types

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, sbyte> config,
            CultureInfo culture)
        {
            return (config as IPropertyPrintingConfig<TOwner>).SetCultureFor<sbyte>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, byte> config,
            CultureInfo culture)
        {
            return (config as IPropertyPrintingConfig<TOwner>).SetCultureFor<byte>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, short> config,
            CultureInfo culture)
        {
            return (config as IPropertyPrintingConfig<TOwner>).SetCultureFor<short>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, ushort> config,
            CultureInfo culture)
        {
            return (config as IPropertyPrintingConfig<TOwner>).SetCultureFor<ushort>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> config,
            CultureInfo culture)
        {
            return (config as IPropertyPrintingConfig<TOwner>).SetCultureFor<int>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, uint> config,
            CultureInfo culture)
        {
            return (config as IPropertyPrintingConfig<TOwner>).SetCultureFor<uint>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> config,
            CultureInfo culture)
        {
            return (config as IPropertyPrintingConfig<TOwner>).SetCultureFor<long>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, ulong> config,
            CultureInfo culture)
        {
            return (config as IPropertyPrintingConfig<TOwner>).SetCultureFor<ulong>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> config,
            CultureInfo culture)
        {
            return (config as IPropertyPrintingConfig<TOwner>).SetCultureFor<float>(culture);
        }


        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> config,
            CultureInfo culture)
        {
            return (config as IPropertyPrintingConfig<TOwner>).SetCultureFor<double>(culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, decimal> config,
            CultureInfo culture)
        {
            return (config as IPropertyPrintingConfig<TOwner>).SetCultureFor<decimal>(culture);
        }

        #endregion
        
        public static PrintingConfig<TOwner> TrimmingToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> config,
            int maxLen)
        {
            return (config as IPropertyPrintingConfig<TOwner>).TrimmingToLength(maxLen);
        }
    }
}