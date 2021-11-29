using System;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TType> : IMemberPrintingConfig<TOwner, TType>
    {
        private readonly PrintingConfig<TOwner> parentConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TType, string> serializationRule)
        {
            return new PrintingConfig<TOwner>(parentConfig, typeof(TType), serializationRule);
        }
    }
}
