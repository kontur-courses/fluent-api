using System;

namespace ObjectPrinting
{
    public class CycleMemberConfig<TOwner, TMember> : INestedPrintingConfig<TOwner, TMember>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public CycleMemberConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TMember, string> customSerializer)
        {
            printingConfig.AddCustomCycleMemberSerializer(typeof(TMember), customSerializer);
            return printingConfig;
        }
    }
}