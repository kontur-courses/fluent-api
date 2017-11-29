using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> WithCulture<TOwner>(
            this TypePrintingConfig<TOwner, int> config,
            CultureInfo cultureInfo)
        {
            return WithCultureInternal(config, cultureInfo);
        }

        public static PrintingConfig<TOwner> WithCulture<TOwner>(
            this TypePrintingConfig<TOwner, long> config,
            CultureInfo cultureInfo)
        {
            return WithCultureInternal(config, cultureInfo);
        }

        public static PrintingConfig<TOwner> WithCulture<TOwner>(
            this TypePrintingConfig<TOwner, double> config,
            CultureInfo cultureInfo)
        {
            return WithCultureInternal(config, cultureInfo);
        }

        public static PrintingConfig<TOwner> WithCulture<TOwner>(
            this TypePrintingConfig<TOwner, float> config,
            CultureInfo cultureInfo)
        {
            return WithCultureInternal(config, cultureInfo);
        }

        public static PrintingConfig<TOwner> WithCulture<TOwner>(
            this TypePrintingConfig<TOwner, decimal> config,
            CultureInfo cultureInfo)
        {
            return WithCultureInternal(config, cultureInfo);
        }

        public static PrintingConfig<TOwner> ShrinkedToLength<TOwner>(
            this TypePrintingConfig<TOwner, string> config,
            int length)
        {
            var parentConfig = config.GetParentConfig();
            parentConfig.SetTypeTransformationRule<string>(
                str => str.Substring(0, Math.Min(str.Length, length)), TypeTransformations.Feature);
            return parentConfig;
        }

        private static PrintingConfig<TOwner> GetParentConfig<TOwner, T>(this IChildPrintingConfig<TOwner, T> config)
            => config.ParentConfig;

        private static PrintingConfig<TOwner> WithCultureInternal<TOwner, T>(
            this TypePrintingConfig<TOwner, T> config,
            CultureInfo cultureInfo) where T : IFormattable
        {
            var parentConfig = config.GetParentConfig();
            parentConfig.SetTypeTransformationRule<T>(number => number.ToString(null, cultureInfo), TypeTransformations.Feature);
            return parentConfig;
        }
    }
}