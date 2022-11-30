using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        public PrintingConfig<TOwner> ParentConfig => printingConfig;

        private Func<TPropType, string> printFunc;
        private CultureInfo culture;
        public int StrTrimLength { get; set; }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            StrTrimLength = int.MaxValue;
            culture = CultureInfo.CurrentCulture;   
            printFunc = (prop) => Convert.ToString(prop, culture);
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> SetSerialization(Func<TPropType, string> print)
        {
            printFunc = print;
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            this.culture = culture;
            return printingConfig;
        }

        public string GetProperty(object obj)
        {
            var prop = printFunc((TPropType) obj);
            return prop.Substring(0, Math.Min(prop.Length, StrTrimLength));
        }
    }
}
