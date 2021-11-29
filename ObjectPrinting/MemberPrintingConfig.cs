using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class MemberPrintingConfig<TOwner, TMember> : INestedPrintingConfig<TOwner, TMember>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo selectedMember;

        public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo selectedMember)
        {
            this.printingConfig = printingConfig;
            this.selectedMember = selectedMember;
        }

        public PrintingConfig<TOwner> Using(Func<TMember, string> customSerializer)
        {
            printingConfig.AddCustomMemberSerializer(selectedMember, customSerializer);
            return printingConfig;
        }
    }
}