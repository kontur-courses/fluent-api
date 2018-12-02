using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly string name;
        internal readonly PrintingConfig<TOwner> PrintingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string name = null)
        {
            PrintingConfig = printingConfig;
            this.name = name;
        }

        internal void AddPrintingFunction(Func<TPropType, string> print)
        {
            string PrintingFunction(object property) => print((TPropType) property);
            if (name != null)
                PrintingConfig.PrintingFunctionsByName[name] = PrintingFunction;
            else
                PrintingConfig.PrintingFunctionsByType[typeof(TPropType)] = PrintingFunction;
        }

        internal void AddCulture(CultureInfo culture)
        {
            if (name != null)
                PrintingConfig.CulturesByName[name] = culture;
            else
                PrintingConfig.CulturesByType[typeof(TPropType)] = culture;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            AddPrintingFunction(print);
            return PrintingConfig;
        }
    }
}