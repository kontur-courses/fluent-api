using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class MemberConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly SerializerSettings settings;
        private readonly MemberInfo memberInfo;

        public MemberConfig(PrintingConfig<TOwner> printingConfig, MemberInfo member, SerializerSettings serializerSettings)
        {
            parentConfig = printingConfig;
            settings = serializerSettings;
            memberInfo = member;
        }

        public PrintingConfig<TOwner> PrintAs(Func<TPropType, string> print)
        {
            settings.CustomMembs.Add(memberInfo, x => print((TPropType)x));
            return parentConfig;
        }

        public PrintingConfig<TOwner> IgnoreProperty()
        {
            settings.MembersToIgnor.Add(memberInfo);
            return parentConfig;
        }
    }
}
