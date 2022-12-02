using ObjectPrinting.Solved;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class MemberConfig<TOwner, TPropType>
    {
        public PrintingConfig<TOwner> ParentConfig { get; private set; }
        private SerializerSettings settings;
        private MemberInfo memberInfo;

        public MemberConfig(PrintingConfig<TOwner> printingConfig, MemberInfo member, SerializerSettings serializerSettings)
        {
            ParentConfig = printingConfig;
            this.settings = serializerSettings;
            memberInfo = member;
        }

        public PrintingConfig<TOwner> PrintAs(Func<TPropType, string> print)
        {
            settings.CustomMembs.Add(memberInfo, x => print((TPropType)x));
            return ParentConfig;
        }

        public PrintingConfig<TOwner> IgnoreProperty()
        {
            settings.MembersToIgnor.Add(memberInfo);
            return ParentConfig;
        }
    }
}
