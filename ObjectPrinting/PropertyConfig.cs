using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ObjectPrinting
{
    public class PropertyConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        public PropertyInfo Properties { get; set; }
        public PropertyConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
            Properties = null;
        }
        public PropertyConfig(PrintingConfig<TOwner> printingConfig,PropertyInfo properties)
        {
            this.printingConfig = printingConfig;
            Properties=properties;
        }


        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            printingConfig.typesForPrintWithSpec[typeof(TPropType)] = print;
            return printingConfig;
        }
 
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        PropertyInfo Properties { get; set; }
    }
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, T>(this PropertyConfig<TOwner, T> config,
            CultureInfo culture, string format = null) where T : IFormattable
        {
            return config.Using(type => type.ToString(format, culture));
        }

        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyConfig<TOwner, string> propertyConfig, int maxLen) 
        {
            var config = (IPropertyPrintingConfig<TOwner, string>)propertyConfig;
            var cfe = config.Properties;
            config.ParentConfig.PropertyLenForString[config.Properties] = maxLen;
            return config.ParentConfig;
 
        }
    }
}
