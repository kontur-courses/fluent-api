using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            string trimToLength(string str) => str.Length < maxLen ? str : str.Substring(0, maxLen);
            var propPrintCfg = (IPropertyPrintingConfig<TOwner, string>)propConfig;
            Func<object, string> objToStr = propPrintCfg.Config.Printing.ContainsKey(propPrintCfg.Member) ?
                propPrintCfg.Config.Printing[typeof(string)] :
                obj => obj.ToString();
            propPrintCfg.Config.Printing[propPrintCfg.Member] = obj => trimToLength(objToStr(obj));
            return propPrintCfg.ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig, CultureInfo culture) =>
            UsingCulture(propConfig, culture);

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig, CultureInfo culture) =>
            UsingCulture(propConfig, culture);

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propConfig, CultureInfo culture) =>
            UsingCulture(propConfig, culture);

        private static PrintingConfig<TOwner> UsingCulture<TOwner, TPropType>(PropertyPrintingConfig<TOwner, TPropType> propConfig, CultureInfo culture)
            where TPropType : IConvertible
        {
            var printingConfig = (IPropertyPrintingConfig<TOwner, TPropType>)propConfig;
            printingConfig.Config.PrintingWithCulture[printingConfig.Member] = obj => ((IConvertible)obj).ToString(culture);
            return printingConfig.ParentConfig;
        }
    }
}