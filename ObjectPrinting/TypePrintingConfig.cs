using System;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TMember> : INestedPrintingConfig<TOwner, TMember>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
            => this.printingConfig = printingConfig;


        public PrintingConfig<TOwner> Using(Func<TMember, string> customSerializer)
        {
            return printingConfig.AddCustomTypeSerializer(typeof(TMember), customSerializer);
        }
    }
}