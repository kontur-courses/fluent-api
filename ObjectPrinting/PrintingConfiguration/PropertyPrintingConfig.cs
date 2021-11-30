using System;
using System.Reflection;

namespace ObjectPrinting.PrintingConfiguration
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IInnerPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        private readonly MemberInfo selectedMember;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo selectedMember)
        {
            this.printingConfig = printingConfig;
            this.selectedMember = selectedMember;
        }

        PrintingConfig<TOwner> IInnerPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
        {
            printingConfig.AddAlternativeMemberSerializer(selectedMember, serializer);
            return printingConfig;
        }
    }
}