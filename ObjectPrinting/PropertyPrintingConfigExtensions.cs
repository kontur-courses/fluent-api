using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> inner,
            CultureInfo culture) => AddCulture(inner, culture, typeof(int));

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> inner,
            CultureInfo culture) => AddCulture(inner, culture, typeof(long));

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> inner,
            CultureInfo culture) => AddCulture(inner, culture, typeof(double));

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, short> inner,
            CultureInfo culture) => AddCulture(inner, culture, typeof(short));

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, ulong> inner,
            CultureInfo culture) => AddCulture(inner, culture, typeof(ulong));

        public static PrintingConfig<TOwner> Trim<TOwner>(this PropertyPrintingConfig<TOwner, string> inner,
            int size)
        {
            if (size <= 0)
                throw new ArgumentException("size must be bigger then 0");
            var printingConfig = ((IPropertyPrintingConfig<TOwner>)inner).PrintingConfig;
            printingConfig.StringTrim = size;
            return printingConfig;
        }

        private static PrintingConfig<TOwner> AddCulture<TOwner>(IPropertyPrintingConfig<TOwner> inner,
            CultureInfo culture, Type type)
        {
            var printingConfig = inner.PrintingConfig;
            ((IPrintingConfig<TOwner>)printingConfig).AddTypeCulture(type, culture);
            return printingConfig;
        }
    }
}