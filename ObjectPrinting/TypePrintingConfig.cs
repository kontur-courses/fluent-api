using System;
using System.Collections.Generic;
using System.Text;
using ObjectPrinting.Config;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropType> : ITypePrintingConfig<TOwner, TPropType>
    {
        private readonly IPrintingConfig<TOwner> printingConfig;

        IPrintingConfig<TOwner> IConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            printingConfig.TypeSerialization[typeof(TPropType)] = print;
            return (PrintingConfig<TOwner>)printingConfig;
        }
    }
}
