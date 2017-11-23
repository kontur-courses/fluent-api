using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> SetCulture<TOwner>(
            this TypePrintingConfig<TOwner, int> config,
            CultureInfo cultureInfo)
        {
            var parentConfig = ((IChildPrintingConfig<TOwner, int>)config).ParentConfig;
            parentConfig.SetTypeTransformationRule<int>(number => number.ToString(cultureInfo));
            return parentConfig;
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(
            this TypePrintingConfig<TOwner, long> config,
            CultureInfo cultureInfo)
        {
            var parentConfig = ((IChildPrintingConfig<TOwner, long>)config).ParentConfig;
            parentConfig.SetTypeTransformationRule<long>(number => number.ToString(cultureInfo));
            return parentConfig;
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(
            this TypePrintingConfig<TOwner, double> config,
            CultureInfo cultureInfo)
        {
            var parentConfig = ((IChildPrintingConfig<TOwner, double>)config).ParentConfig;
            parentConfig.SetTypeTransformationRule<double>(number => number.ToString(cultureInfo));
            return parentConfig;
        }

        public static PrintingConfig<TOwner> ShrinkToLength<TOwner>(
            this TypePrintingConfig<TOwner, string> config,
            int length)
        {
            var parentConfig = ((IChildPrintingConfig<TOwner, string>) config).ParentConfig;
            parentConfig.SetTypeTransformationRule<string>(str => str.Substring(0, Math.Min(str.Length, length)));
            return parentConfig;
        }
    }
}