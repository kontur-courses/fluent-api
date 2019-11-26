using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    public static class PrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> SetFormat<TOwner>(
            this PropertyPrintingConfig<TOwner, int> config, CultureInfo cultureInfo)
        {
            var parent = (config as IPropertyPrintingConfig<TOwner>).ParentConfig as IPrintingConfig<TOwner>;
            parent.DigitTypeToCulture[typeof(int)] = cultureInfo;  
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> SetFormat<TOwner>(
            this PropertyPrintingConfig<TOwner, double> config, CultureInfo cultureInfo)
        {
            var parent = (config as IPropertyPrintingConfig<TOwner>).ParentConfig as IPrintingConfig<TOwner>;
            parent.DigitTypeToCulture[typeof(double)] = cultureInfo;
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> SetFormat<TOwner>(
            this PropertyPrintingConfig<TOwner, float> config, CultureInfo cultureInfo)
        {
            var parent = (config as IPropertyPrintingConfig<TOwner>).ParentConfig as IPrintingConfig<TOwner>;
            parent.DigitTypeToCulture[typeof(float)] = cultureInfo;
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Cut<TOwner>(
            this PropertyPrintingConfig<TOwner, string> config, int amount)
        {
            var parent = (config as IPropertyPrintingConfig<TOwner>).ParentConfig as IPrintingConfig<TOwner>;
            var propertyInfo = (config as IPropertyPrintingConfig<TOwner>).ConfiguredProperty;

            parent.PropertyToLength[propertyInfo] = amount;

            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static string Print<TOwner>(
            this TOwner formattedObject)
        {
            return new PrintingConfig<TOwner>(formattedObject).Print();
        }

        public static PrintingConfig<TOwner> ConfigureFormat<TOwner>(
            this TOwner formattedObject)
        {
            return new PrintingConfig<TOwner>(formattedObject);
        }
    }
}
