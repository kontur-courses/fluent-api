using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
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
                printingConfig.AddPropertryPrinter(propName, x => print((TPropType)x));
          return printingConfig;
        }

        //public PrintingConfig<TOwner> Using(CultureInfo culture)
        //{
        //    return printingConfig;
        //}

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        public PrintingConfig<TOwner> Using(Func<TPropType, string> print);
    }
}