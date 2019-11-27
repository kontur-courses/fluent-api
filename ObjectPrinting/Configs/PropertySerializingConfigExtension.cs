using System;
using System.Globalization;

namespace ObjectPrinting.Configs
{
    public static class PropertySerializingConfigExtension
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializingConfig<TOwner, int> config,
            CultureInfo culture)
        {
            return config.Using(num => num.ToString(culture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializingConfig<TOwner, short> config,
            CultureInfo culture)
        {
            return config.Using(num => num.ToString(culture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializingConfig<TOwner, ushort> config,
            CultureInfo culture)
        {
            return config.Using(num => num.ToString(culture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializingConfig<TOwner, uint> config,
            CultureInfo culture)
        {
            return config.Using(num => num.ToString(culture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializingConfig<TOwner, long> config,
            CultureInfo culture)
        {
            return config.Using(num => num.ToString(culture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializingConfig<TOwner, ulong> config,
            CultureInfo culture)
        {
            return config.Using(num => num.ToString(culture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializingConfig<TOwner, float> config,
            CultureInfo culture)
        {
            return config.Using(num => num.ToString(culture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializingConfig<TOwner, double> config,
            CultureInfo culture)
        {
            return config.Using(num => num.ToString(culture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializingConfig<TOwner, decimal> config,
            CultureInfo culture)
        {
            return config.Using(num => num.ToString(culture));
        }

        public static PrintingConfig<TOwner> Take<TOwner>(this PropertySerializingConfig<TOwner, string> config,
            int length)
        {
            return config.Using(s => s.Substring(0, Math.Min(length, s.Length)));
        }
    }
}