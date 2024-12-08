using System;
using System.Globalization;
using ObjectPrinting.Configs;

namespace ObjectPrinting.Utilits.Configs
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimEnd<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int length)
        {
            var printingConfig = propConfig.PrintingConfig;
            propConfig.PrintingConfig
                .AddTrimmedPropertiesProcessing(length, propConfig.MemberInfo!);
            return printingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner, TPropType> propConfig,
            CultureInfo culture) where TPropType : IFormattable
            => propConfig.Using(x => x.ToString(null, culture));
    }
}