using System;
using System.Reflection;

namespace ObjectPrinting.Configs
{
    public class MemberConfig<TOwner, TMember> : INestedPrintingConfig<TOwner, TMember>
    {
        private readonly PrintingConfig<TOwner> config;
        private readonly SerializationSettings settings;
        private readonly MemberInfo memberInfo;

        public MemberConfig(PrintingConfig<TOwner> config, SerializationSettings settings, MemberInfo memberInfo)
        {
            this.config = config;
            this.settings = settings;
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> With(Func<TMember, string> serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            settings.SetSerializer(memberInfo, serializer);
            return config;
        }
    }
}