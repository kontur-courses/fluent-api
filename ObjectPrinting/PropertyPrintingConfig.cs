using System;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string propertyName;
        private readonly Func<Func<TPropType, string>, PrintingConfig<TOwner>> currentUsing;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propertyName)
        {
            this.printingConfig = printingConfig;
            this.propertyName = propertyName;
            currentUsing = UsingPropertySerialize;
        }
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
            currentUsing = UsingTypeSerialize;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            return currentUsing(print);
        }

        private PrintingConfig<TOwner> UsingPropertySerialize(Func<TPropType, string> print)
        {
            printingConfig.AddPropertySerialize(print, propertyName);
            return printingConfig;
        }

        private PrintingConfig<TOwner> UsingTypeSerialize(Func<TPropType, string> print)
        {
            printingConfig.AddTypeSerialize(print);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        string IPropertyPrintingConfig<TOwner, TPropType>.PropertyName => propertyName;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        string PropertyName { get; }
    }
}