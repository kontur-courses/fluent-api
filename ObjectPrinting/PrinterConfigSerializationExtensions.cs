using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PrinterConfigSerializationExtensions
    {
        public static PrintingConfig<TOwner> SetCulture<TOwner, TSerialization>(
            this PrinterConfigSerialization<TOwner, TSerialization> config, CultureInfo cultureInfo)
            where TSerialization : IFormattable
        {
            return config.SetSerialization(x => x.ToString(null, cultureInfo));
        }

        public static PrintingConfig<TOwner> SetMaxStringLength<TOwner>(
            this PrinterConfigSerialization<TOwner, string> config, int maxLength)
        {
            return config.SetSerialization(x => x[..maxLength]);
        }
    }
}