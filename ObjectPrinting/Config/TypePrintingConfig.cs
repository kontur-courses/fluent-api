using System;

namespace ObjectPrinting.Config
{
    public class TypePrintingConfig<TOwner, TPropType> : IPrintingConfig<TOwner, TPropType>
    {
        public PrintingConfig<TOwner> ParentConfig { get; }

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            ParentConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            ParentConfig.OverrideTypePrinting(print);

            return ParentConfig;
        }
    }
}