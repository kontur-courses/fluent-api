using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> resializeFuction)
        {
            return printingConfig;
        }
             
        public PrintingConfig<TOwner> Using(CultureInfo cultureInfo)
        {
            return printingConfig;
        }

        public PrintingConfig<TOwner> CutToLenght(int i)
        {
            return new PrintingConfig<TOwner>();
        }
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}