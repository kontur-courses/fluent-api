using System;
using System.Reflection;

namespace ObjectPrinting.Configs
{
    public class MemberPrintingConfig<TOwner, TMemberType>
    {
        internal PrintingConfig<TOwner> ParentConfig { get; }
        private readonly MemberInfo member;
        private readonly GlobalConfig config;

        public MemberPrintingConfig(PrintingConfig<TOwner> parentConfig, MemberInfo member)
        {
            ParentConfig = parentConfig;
            config = parentConfig.Config;
            this.member = member;
        }

        public PrintingConfig<TOwner> Using(Func<TMemberType, string> serializeFunc)
        {
            config.AddAlternativeMemberSerialization(member, 
                obj => serializeFunc.Invoke((TMemberType)obj));
            return ParentConfig;
        }
    }
}