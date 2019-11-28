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

        #region
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> config, CultureInfo currentCulture)
        {
            return config.Using(number => number.ToString(currentCulture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, byte> config, CultureInfo currentCulture)
        {
            return config.Using(number => number.ToString(currentCulture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, sbyte> config, CultureInfo currentCulture)
        {
            return config.Using(number => number.ToString(currentCulture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, short> config, CultureInfo currentCulture)
        {
            return config.Using(number => number.ToString(currentCulture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, ushort> config, CultureInfo currentCulture)
        {
            return config.Using(number => number.ToString(currentCulture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, uint> config, CultureInfo currentCulture)
        {
            return config.Using(number => number.ToString(currentCulture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> config, CultureInfo currentCulture)
        {
            return config.Using(number => number.ToString(currentCulture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, ulong> config, CultureInfo currentCulture)
        {
            return config.Using(number => number.ToString(currentCulture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, decimal> config, CultureInfo currentCulture)
        {
            return config.Using(number => number.ToString(currentCulture));
        }
        #endregion

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
