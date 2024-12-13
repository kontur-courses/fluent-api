using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProp>
    {
        private readonly SerrializeConfig serrializeConfig;
        private readonly MemberInfo memberInfo;

        public PropertyPrintingConfig(SerrializeConfig serrializeConfig, MemberInfo memberInfo = null)
        {
            this.serrializeConfig = new SerrializeConfig(serrializeConfig);
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TProp, string> serrializer)
        {
            if (memberInfo == null)
                serrializeConfig.AddTypeSerializer(typeof(TProp), serrializer);
            else
                serrializeConfig.AddMemberSerializer(memberInfo, serrializer);

            return new PrintingConfig<TOwner>(serrializeConfig);
        }
    }
}
