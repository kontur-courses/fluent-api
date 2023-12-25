using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PrintingExtension
    {
        public static PrintingConfig<TOwner> Truncate<TOwner>(this PropertyConfig<TOwner, string> config, int startPos,
            int length)
        {
            return config.SerializeAs(s => s.Substring(startPos, length));
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this TypeConfig<TOwner, double> config,
            CultureInfo info)
        {
            return config.SerializeAs(d => d.ToString(info));
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this PropertyConfig<TOwner, double> config,
            CultureInfo info)
        {
            return config.SerializeAs(d => d.ToString(info));
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this TypeConfig<TOwner, DateTime> config,
            CultureInfo info)
        {
            return config.SerializeAs(date => date.ToString(info));
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this PropertyConfig<TOwner, DateTime> config,
            CultureInfo info)
        {
            return config.SerializeAs(date => date.ToString(info));
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this TypeConfig<TOwner, string> config,
            CultureInfo info)
        {
            return config.SerializeAs(date => date.ToString(info));
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this PropertyConfig<TOwner, string> config,
            CultureInfo info)
        {
            return config.SerializeAs(date => date.ToString(info));
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this TypeConfig<TOwner, int> config, CultureInfo info)
        {
            return config.SerializeAs(date => date.ToString(info));
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this PropertyConfig<TOwner, int> config,
            CultureInfo info)
        {
            return config.SerializeAs(date => date.ToString(info));
        }
    }
}