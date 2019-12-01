using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> Trim<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLength)
        {
            return propConfig.Using(str =>
                str.Length > maxLength
                    ? str.Substring(0, maxLength)
                    : str);
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig,
            CultureInfo culture)
        {
            return propConfig.Using(x => ((int)(object)x).ToString(culture));            
        }        
                
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig,
            CultureInfo culture)
        {
            return propConfig.Using(x => ((double)(object)x).ToString(culture));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propConfig,
            CultureInfo culture)
        {
            return propConfig.Using(x => ((float)(object)x).ToString(culture));
        } 
    }
}
 