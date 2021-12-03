using System;
using System.Globalization;

namespace Printer
{
    public static class PrinterConfigExtensions
    {
        public static PrintingConfig<TOwner, TType> SetCulture<TOwner, TType>(
            this PrintingConfig<TOwner, TType> config, CultureInfo cultureInfo)
            where TType : IFormattable
        {
            config.SetCustomSerializing(x => x.ToString(null, cultureInfo));
            return config;
        }

        public static PrintingConfig<TOwner, string> LengthOfString<TOwner>(
            this PrintingConfig<TOwner, string> config, int maxLength)
        {
            config.SetCustomSerializing(x => x[..maxLength]);
            return config;
        }
    }
}