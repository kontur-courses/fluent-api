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
            var parentConfig = ExtractParentConfig(config);
            parentConfig.SetTypeTransformationRule<int>(number => number.ToString(cultureInfo), TransformationType.TypeFeature);
            return parentConfig;
        }

        public static PrintingConfig<TOwner> WithCulture<TOwner>(
            this TypePrintingConfig<TOwner, long> config,
            CultureInfo cultureInfo)
        {
            var parentConfig = ExtractParentConfig(config);
            parentConfig.SetTypeTransformationRule<long>(number => number.ToString(cultureInfo), TransformationType.TypeFeature);
            return parentConfig;
        }

        public static PrintingConfig<TOwner> WithCulture<TOwner>(
            this TypePrintingConfig<TOwner, double> config,
            CultureInfo cultureInfo)
        {
            var parentConfig = ExtractParentConfig(config);
            parentConfig.SetTypeTransformationRule<double>(number => number.ToString(cultureInfo), TransformationType.TypeFeature);
            return parentConfig;
        }

        public static PrintingConfig<TOwner> WithCulture<TOwner>(
            this TypePrintingConfig<TOwner, float> config,
            CultureInfo cultureInfo)
        {
            var parentConfig = ExtractParentConfig(config);
            parentConfig.SetTypeTransformationRule<float>(number => number.ToString(cultureInfo), TransformationType.TypeFeature);
            return parentConfig;
        }

        public static PrintingConfig<TOwner> WithCulture<TOwner>(
            this TypePrintingConfig<TOwner, decimal> config,
            CultureInfo cultureInfo)
        {
            var parentConfig = ExtractParentConfig(config);
            parentConfig.SetTypeTransformationRule<decimal>(
                number => number.ToString(cultureInfo), TransformationType.TypeFeature);
            return parentConfig;
        }

        public static PrintingConfig<TOwner> ShrinkedToLength<TOwner>(
            this TypePrintingConfig<TOwner, string> config,
            int length)
        {
            var parentConfig = ExtractParentConfig(config);
            parentConfig.SetTypeTransformationRule<string>(
                str => str.Substring(0, Math.Min(str.Length, length)), TransformationType.TypeFeature);
            return parentConfig;
        }

        private static PrintingConfig<TOwner> ExtractParentConfig<TOwner, T>(IChildPrintingConfig<TOwner, T> config)
            => config.ParentConfig;
    }
}