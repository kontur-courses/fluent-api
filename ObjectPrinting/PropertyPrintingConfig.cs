using System;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string propertyName;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propertyName = null)
        {
            this.printingConfig = printingConfig;
            this.propertyName = propertyName;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> function)
        {
            if (propertyName == null)
                ((IPrintingConfig<TOwner>)printingConfig).AddSpecialTypeSerializing(typeof(TPropType), function);
            else
                ((IPrintingConfig<TOwner>)printingConfig).AddSpecialPropertySerializing(propertyName, function);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.PrintingConfig => printingConfig;
    }
}