using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropType>: ITypePrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        PrintingConfig<TOwner> ITypePrintingConfig<TOwner>.PrintingConfig => printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public TypePrintingConfig<TOwner, TPropType> Using(Func<TPropType, string> serializationType)
        {
            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> Trunc<String>(int startId, int length)
        {
            return this;
        }

        public PrintingConfig<TOwner> End()
        {
            return printingConfig;
        }
    }
}