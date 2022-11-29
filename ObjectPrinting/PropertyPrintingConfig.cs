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


        public Func<TPropType, string> printFunc = (prop) => prop.ToString();
        public int strTrimLength = int.MaxValue;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> SetSerialization(Func<TPropType, string> print)
        {
            printFunc = print;
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            return printingConfig;
        }

        public string GetProperty(object obj)
        {
            var prop = printFunc((TPropType) obj);
            return prop.Substring(0, Math.Min(prop.Length, strTrimLength));
        }
    }
}
