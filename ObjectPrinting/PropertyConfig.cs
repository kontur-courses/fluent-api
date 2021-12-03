using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyConfig<TOwner, TMember>
    {
        public readonly MemberInfo member;
        public readonly PrintingConfig<TOwner> printingConfig;

        public PropertyConfig(PrintingConfig<TOwner> parentConfig, MemberInfo member)
        {
            this.member = member;
            printingConfig = parentConfig;
        }

        public PrintingConfig<TOwner> Using(Func<MemberInfo, string> serializer)
        {
            printingConfig.AddAlternativeMemberSerializer(member, serializer);
            return printingConfig;
        }
    }
}