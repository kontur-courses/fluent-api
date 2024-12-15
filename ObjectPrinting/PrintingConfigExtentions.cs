using System;
using System.Globalization;
using System.Linq;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PropertyPrintingConfig<T, string> Truncate<T>(this PropertyPrintingConfig<T,string> config, int symbolsCount)
        {
            config.RefineSerializer<string>(str =>
            {
                if (symbolsCount > str.Length)
                    return "";
                return new string(str.Take(symbolsCount).ToArray());
            });
            return config;
        }

        public static PropertyPrintingConfig<T, double> WithCulture<T>(this PropertyPrintingConfig<T, double> config, CultureInfo culture)
        {
            config.RefineSerializer<double>(s => double.TryParse(s, out var prop) ? prop.ToString(culture) : s);
            return config;
        }
        public static PropertyPrintingConfig<T, DateTime> WithCulture<T>(this PropertyPrintingConfig<T, DateTime> config, CultureInfo culture)
        {
            config.RefineSerializer<DateTime>(s => DateTime.TryParse(s, out var prop) ? prop.ToString(culture) : s);
            return config;
        }
    }
}
