using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> config, CultureInfo currentCulture)
        {
            return config.Using(number => number.ToString(currentCulture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> config, CultureInfo currentCulture)
        {
            return config.Using(number => number.ToString(currentCulture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> config, CultureInfo currentCulture)
        {
            return config.Using(number => number.ToString(currentCulture));
        }

        public static PrintingConfig<TOwner> TrimToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> config, int length)
        {
            if (length < 0)
            {
                throw new ArgumentException("Length must be positive number");
            }
            return config.Using(a => a.Substring(0, a.Length > length ? length : a.Length));
        }
    }
}
