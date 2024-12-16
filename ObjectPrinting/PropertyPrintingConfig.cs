using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType>
        : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string propName;
        
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propName = null)
        {
            this.printingConfig = printingConfig;
            this.propName = propName;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if(propName == null)
                this.printingConfig.AddTypePrinter(print);
            else 
                printingConfig.AddPropertyPrinter(propName, x => print((TPropType)x));
          return printingConfig;
        }
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        public PrintingConfig<TOwner> Using(Func<TPropType, string> print);
    }
}