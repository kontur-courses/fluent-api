using System;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            var settings = ((IPrintingConfig) printingConfig).Settings;
            return new PrintingConfig<TOwner>(settings.AddWayToSerializeType(typeof(TPropType),
                obj => print((TPropType) obj)));
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }
}