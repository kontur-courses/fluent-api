namespace ObjectPrinting
{
    using System;
    using System.Globalization;

    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config) =>
            config(ObjectPrinter.For<T>())
                .PrintToString(obj);

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> config,
            int maxLen) =>
            config.Using(s => s.Substring(0, Math.Min(maxLen, s.Length)));

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, int> config,
            CultureInfo culture) =>
            config.Using(i => i.ToString(culture));

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, double> config,
            CultureInfo culture) =>
            config.Using(i => i.ToString(culture));

        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, long> config,
            CultureInfo culture) =>
            config.Using(i => i.ToString(culture));

        private static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this IPropertyPrintingConfig<TOwner, TPropType> config,
            Func<TPropType, string> printer) =>
            ((IPrintingConfig<TOwner>)config.ParentConfig).With(printer);
    }
}
