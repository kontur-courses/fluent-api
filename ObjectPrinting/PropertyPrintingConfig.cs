using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string name;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string name = "")
        {
            this.printingConfig = printingConfig;
            this.name = name;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            Func<object, string> func = o => print((TPropType) o);
            if (name.Equals(""))
                ((IPrintingConfig) printingConfig).PrintingFunctionsByType.Add(typeof(TPropType), func);
            else
                ((IPrintingConfig) printingConfig).PrintingFunctionsByName.Add(name, func);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            ((IPrintingConfig) printingConfig).PrintingFunctionsByType.Add(typeof(TPropType),
                new Func<TPropType, string>(obj => Convert.ToString(obj, culture)));
            return printingConfig;
        }
    }
}