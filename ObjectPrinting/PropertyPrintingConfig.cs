using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner,TType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TType, string> func) 
        {
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using<T>(CultureInfo currentCulture) 
        {
            return printingConfig;
        }

        public PrintingConfig<TOwner> CutTo(int index)
        {
            return printingConfig;
        }
    }
    
    public static class Extensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner,double> config, CultureInfo currentCulture) 
        {
            
        }
    }
}