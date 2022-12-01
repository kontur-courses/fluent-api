using System;
using System.Reflection;

namespace ObjectPrinting.PrintingConfiguration
{
    public class MemberPrintingConfig<TOwner, TMemberType>
    {
        public PrintingConfig<TOwner> PrintingConfig { get; }
        public MemberInfo MemberInfo { get; }

        public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo memberInfo)
        {
            PrintingConfig = printingConfig;
            MemberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> ChangeSerialization(Func<TMemberType, string> func)
        {
            PrintingConfig.AddSerializationRule(MemberInfo, func);
            return PrintingConfig;
        }
    }
}