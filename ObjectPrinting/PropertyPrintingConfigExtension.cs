using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {     
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig, CultureInfo culture)
        {
            var parenConfig = (IPrintingConfig<TOwner>)((IPropertyPrintingConfig<TOwner, int>)propConfig).ParentConfig;
            parenConfig.CulturesForNumbers[typeof(int)] = culture;
            return (PrintingConfig<TOwner>)parenConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig, CultureInfo culture)
        {
            var parenConfig = (IPrintingConfig<TOwner>)((IPropertyPrintingConfig<TOwner, double>)propConfig).ParentConfig;
            parenConfig.CulturesForNumbers[typeof(double)] = culture;
            return (PrintingConfig<TOwner>)parenConfig;
        }


        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> propConfig, CultureInfo culture)
        {
            var parenConfig = (IPrintingConfig<TOwner>)((IPropertyPrintingConfig<TOwner, long>)propConfig).ParentConfig;
            parenConfig.CulturesForNumbers[typeof(long)] = culture;
            return (PrintingConfig<TOwner>)parenConfig;
        }

    }
}