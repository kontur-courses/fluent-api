using System.Globalization;
using System;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            var castedFunc = new Func<object, string>((obj) => print((TPropType)obj));
            printingConfig.typeSerializers.Add(typeof(TPropType), castedFunc);
            return printingConfig;
        }
    }
}