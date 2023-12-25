using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TType> : IInnerPrintingConfig<TOwner, TType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        PrintingConfig<TOwner> IInnerPrintingConfig<TOwner, TType>.ParentConfig => printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TType, string> print)
        {
            printingConfig.TypeSerializers[typeof(TType)] = obj => print((TType)obj);
            return printingConfig;
        }
    }
}