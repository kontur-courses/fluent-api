using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string propertyName;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propertyName = null)
        {
            this.printingConfig = printingConfig;
            this.propertyName = propertyName;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (propertyName == null)
                printingConfig.ChangeSerializationForType(typeof(TPropType), print);
            else printingConfig.ChangeSerializationForProperty(propertyName, print);
            return printingConfig;
        }


        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}