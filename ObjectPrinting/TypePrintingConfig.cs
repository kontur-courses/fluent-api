using System;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TType> : ITypePrintingConfig<TOwner, TType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        PrintingConfig<TOwner> ITypePrintingConfig<TOwner, TType>.ParentConfig => printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Use(Func<TType, string> print)
        {
            var configInterface = printingConfig as IPrintingConfig<TOwner>;

            if (configInterface.TypeSerializers.ContainsKey(typeof(TType)))
                configInterface.TypeSerializers[typeof(TType)] = print;
            else
                configInterface.TypeSerializers.Add(typeof(TType), print);

            return printingConfig;
        }
    }
}