using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IInnerPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        
        private readonly MemberInfo selectedMember;

        PrintingConfig<TOwner> IInnerPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo selectedMember)
        {
            this.printingConfig = printingConfig;
            this.selectedMember = selectedMember;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
        {
            printingConfig.AddAlternativeMemberSerializator(selectedMember, serializer);
            return printingConfig;
        }

    }
}
