using System;

namespace ObjectPrinting
{
    public class MemberPrintingConfig<TOwner, TMemberType> : IMemberPrintingConfig<TOwner, TMemberType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig) => this.printingConfig = printingConfig;

        PrintingConfig<TOwner> IMemberPrintingConfig<TOwner, TMemberType>.ParentConfig => printingConfig;

        public PrintingConfig<TOwner> Using(Func<TMemberType, string> alternateMemberSerialisator)
        {
            ((IPrintingConfig)printingConfig).SetAlternateMemberSerialisator(alternateMemberSerialisator);

            return printingConfig;
        }
    }
}