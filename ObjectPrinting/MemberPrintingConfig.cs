using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class MemberPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo memberInfo;

        public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo memberInfo)
        {
            this.printingConfig = printingConfig;
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> typePrint)
        {
            if (memberInfo is not null)
                printingConfig.AddMemberSerializer(memberInfo, typePrint);
            else
                printingConfig.AddTypeSerializer(typeof(TPropType), typePrint);
            return printingConfig;
        }
    }
}