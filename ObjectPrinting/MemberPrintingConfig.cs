using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class MemberPrintingConfig<TOwner, TMemberType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo selectedMember;

        public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo selectedMember)
        {
            this.printingConfig = printingConfig;
            this.selectedMember = selectedMember;
        }
        
        public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TMemberType, string> print)
        {
            Func<object, string> altSerializer = obj => print((TMemberType)obj);

            if (selectedMember == null)
            {
                printingConfig.AddAltTypeSerialzier(typeof(TMemberType), altSerializer);
            }
            else
            {
                printingConfig.AddAltMemberSerializer(selectedMember, altSerializer);
            }
            
            return printingConfig;
        }
    }
}