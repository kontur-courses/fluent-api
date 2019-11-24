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

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig, CultureInfo culture)
        {
            return UsingCulture(propConfig, culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig, CultureInfo culture)
        {
            return UsingCulture(propConfig, culture);
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propConfig, CultureInfo culture)
        {
            return UsingCulture(propConfig, culture);
        }

        public static PrintingConfig<TOwner> UsingCulture<TOwner, TPropType>(PropertyPrintingConfig<TOwner, TPropType> propConfig, CultureInfo culture)
        {
            Func<object, string> getStr = null;
            var type = typeof(TPropType);
            if (type == typeof(int))
                getStr = obj => ((int)obj).ToString(culture);
            else if (type == typeof(float))
                getStr = obj => ((float)obj).ToString(culture);
            else if (type == typeof(double))
                getStr = obj => ((double)obj).ToString(culture);
            else
                throw new ArgumentException($"{typeof(TPropType)} is not numeric type");
            var printingConfig = (IPropertyPrintingConfig<TOwner, TPropType>)propConfig;
            printingConfig.Config.PrintingWithCulture[printingConfig.Member] = getStr;
            return printingConfig.ParentConfig;
        }
    }
}