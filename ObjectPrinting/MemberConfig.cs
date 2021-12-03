using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class MemberConfig<TOwner, TMember>
    {
        public readonly MemberInfo Member;
        public readonly PrintingConfig<TOwner> PrintingConfig;

        public MemberConfig(PrintingConfig<TOwner> parentConfig, MemberInfo member)
        {
            Member = member;
            PrintingConfig = parentConfig;
        }

        public PrintingConfig<TOwner> Using(Func<MemberInfo, string> serializer)
        {
            PrintingConfig.AddAlternativeMemberSerializer(Member, serializer);
            return PrintingConfig;
        }
    }
}