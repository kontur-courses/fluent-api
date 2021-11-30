using System;
using System.Reflection;

namespace ObjectPrinting.Configs
{
    public class MemberConfig<TOwner, TMember> : INestedPrintingConfig<TOwner, TMember>
    {
        private readonly PrintingConfig<TOwner> config;
        private readonly MemberInfo memberInfo;

        public MemberConfig(PrintingConfig<TOwner> config, MemberInfo memberInfo)
        {
            this.config = config;
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> With(Func<TMember, string> serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            config.AddMemberSerializer(memberInfo, serializer);
            return config;
        }
    }
}