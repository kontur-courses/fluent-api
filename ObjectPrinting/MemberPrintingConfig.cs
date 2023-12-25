using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class MemberPrintingConfig<TOwner, TMemberType> : IInnerPrintingConfig<TOwner, TMemberType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        PrintingConfig<TOwner> IInnerPrintingConfig<TOwner, TMemberType>.ParentConfig => printingConfig;
        private readonly MemberInfo memberInfo;

        public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo memberInfo)
        {
            this.printingConfig = printingConfig;
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TMemberType, string> print)
        {
            printingConfig.MemberSerializers[memberInfo] = obj => print((TMemberType)obj);
            return printingConfig;
        }
    }
}