using System;
using System.Globalization;
using ObjectPrinting.Configs;

namespace ObjectPrinting.Extensions
{
    public static class NestingConfigExtensions
    {
        public static PrintingConfig<TOwner> With<TOwner, TType>(
            this INestedPrintingConfig<TOwner, TType> config, CultureInfo cultureInfo)
            where TType : IFormattable
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (cultureInfo == null)
                throw new ArgumentNullException(nameof(cultureInfo));
            return config.With(x => x.ToString(null, cultureInfo));
        }

        public static PrintingConfig<TOwner> WithTrimming<TOwner>(this INestedPrintingConfig<TOwner, string> config,
            int maxLength)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (maxLength <= 0)
                throw new ArgumentException($"length must be positive, but was {maxLength}");

            return config.With(x => x[..Math.Min(maxLength, x.Length)]);
        }
    }
}