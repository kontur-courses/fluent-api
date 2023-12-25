using System;
using System.Globalization;
using System.Reflection;
using ObjectPrinting.Extensions;

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
        
        public PrintingConfig<TOwner> TrimmedToLength(int maxLen)
        {
            var isSerialized = printingConfig.MemberSerializers.TryGetValue(memberInfo, out var prevSerializer);
            printingConfig.MemberSerializers[memberInfo] = isSerialized 
                ? obj => prevSerializer(obj).Truncate(maxLen) 
                : obj => obj.ToString().Truncate(maxLen);
            return printingConfig;
        }
    }
}