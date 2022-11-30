using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyConfig<TOwner, TPropType>  
    {
        internal PropertyInfo Properties { get; }
        internal PrintingConfig<TOwner> PrintingConfig { get; }
        public PropertyConfig(PrintingConfig<TOwner> printingConfig)
        {
            PrintingConfig = printingConfig;
            Properties = null;
        }

        public PropertyConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo properties)
        {
            PrintingConfig = printingConfig;
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
                PrintingConfig.TypesForPrintWithSpec[typeof(TPropType)] = print;
            else
                PrintingConfig.PropertiesForPrintWithSpec[Properties] = print;
            return PrintingConfig;
        }
        
    }
}
