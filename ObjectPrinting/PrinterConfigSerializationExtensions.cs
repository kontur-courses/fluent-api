using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PrinterConfigSerializationExtensions
    {
        public static PrinterConfigSerialization<TOwner, TSer> SetCulture<TOwner, TSer>(
            this PrinterConfigSerialization<TOwner, TSer> config, CultureInfo cultureInfo)
            where TSer : IFormattable
        {
            config.SetSerialization(x => x.ToString(null, cultureInfo));
            return config;
        }

        public static PrinterConfigSerialization<TOwner, string> SetMaxStringLength<TOwner>(
            this PrinterConfigSerialization<TOwner, string> config, int maxLength)
        {
            config.SetSerialization(x => x[..maxLength]);
            return config;
        }
    }
}