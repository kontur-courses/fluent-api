using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner, TPropType> config,
            CultureInfo cultureInfo, int precision = 0) where TPropType : struct, IFormattable
        {
            return config.Using(arg => arg.ToString($"n{precision}", cultureInfo));
        }
    }
}