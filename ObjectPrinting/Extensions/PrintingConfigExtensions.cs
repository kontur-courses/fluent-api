using System;
using ObjectPrinting.PrintingConfig;

namespace ObjectPrinting.Extensions
{
    public static class PrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Use<TOwner, TType>(
            this INestingPrintingConfig<TOwner, TType> nestingConfig, IFormatProvider provider)
            where TType : IFormattable
        {
            if (nestingConfig == null) throw new ArgumentNullException(nameof(nestingConfig));
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            return nestingConfig.Use(value => value.ToString(null, provider));
        }

        public static PrintingConfig<TOwner> UseTrimming<TOwner>(
            this INestingPrintingConfig<TOwner, string> nestingConfig, int maxLength)
        {
            if (nestingConfig == null) throw new ArgumentNullException(nameof(nestingConfig));
            if (maxLength < 0)
                throw new ArgumentException($"Expected {nameof(maxLength)} non negative, bug actual {maxLength}");
            return nestingConfig.Use(value => value[..Math.Min(maxLength, value.Length)]);
        }
    }
}