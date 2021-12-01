using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class MemberPrintingConfig<TOwner, TMemType> : IMemberPrintingConfig<TOwner, TMemType>
    {
        PrintingConfig<TOwner> IMemberPrintingConfig<TOwner, TMemType>.ParentConfig => printingConfig;
        MemberInfo IMemberPrintingConfig<TOwner, TMemType>.MemberInfo => memberInfo;
        
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo memberInfo;

        public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo memberInfo)
        {
            this.printingConfig = printingConfig;
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TMemType, string> typePrint)
        {
            if (memberInfo is not null)
                printingConfig.AddMemberSerializer(memberInfo, typePrint);
            else
                printingConfig.AddTypeSerializer(typeof(TMemType), typePrint);
            return printingConfig;
        }
    }
}