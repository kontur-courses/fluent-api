using System;
using System.Globalization;
using System.Reflection;

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

        public PropertyConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo properties)
        {
            this.printingConfig = printingConfig;
            Properties = properties;
        }

        /// <summary>
        /// Defines settings for including properties
        /// </summary>
        /// <param name="print"> </param>
        /// <returns>PrintingConfig&lt;TOwner&gt;</returns>
        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (Properties == null)
                printingConfig.TypesForPrintWithSpec[typeof(TPropType)] = print;
            else
                printingConfig.PropertiesForPrintWithSpec[Properties] = print;
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }


    public static class PropertyExtensions
    {
        /// <summary>
        /// Defines format settings for including properties
        /// </summary>
        /// <typeparam name="TOwner"></typeparam>
        /// <typeparam name="T">IFormattable type</typeparam>
        /// <param name="config"></param>
        /// <param name="culture">Culture for IFormattable types </param>
        /// <param name="format"></param>
        /// <returns>PrintingConfig&lt;TOwner&gt;</returns>
        public static PrintingConfig<TOwner> Using<TOwner, T>(this PropertyConfig<TOwner, T> config,
            CultureInfo culture, string format = null) where T : IFormattable
        {
            return config.Using(type => type.ToString(format, culture));
        }

        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        /// <summary>
        /// Trim string for object printer
        /// </summary>
        /// <typeparam name="TOwner"></typeparam>
        /// <param name="propertyConfig"></param>
        /// <param name="maxLen">Length maximum setting for string</param>
        /// <returns></returns>
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyConfig<TOwner, string> propertyConfig,
            int maxLen)
        {
            var config = (IPropertyPrintingConfig<TOwner, string>)propertyConfig;

            config.ParentConfig.PropertyLenForString[config.Properties] = maxLen;
            return config.ParentConfig;

        }
    }
}
