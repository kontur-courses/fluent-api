using ObjectPrinting.Solved;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class MemberConfig<TOwner, TPropType> : IConfig<TOwner, TPropType>
    {
        public readonly PrintingConfig<TOwner> printingConfig;
        public PrintingConfig<TOwner> ParentConfig => printingConfig;
        public Settings serializerSettings;
        public MemberInfo propertyInfo;

        public MemberConfig(PrintingConfig<TOwner> printingConfig, MemberInfo member, Settings serializerSettings)
        {
            this.printingConfig = printingConfig;
            this.serializerSettings = serializerSettings;
            propertyInfo = member;
        }

        public PrintingConfig<TOwner> PrintAs(Func<TPropType, string> print)
        {
            var objFunc = new Func<object, string>(x => print((TPropType)x));
            serializerSettings.customMembs.Add(propertyInfo, objFunc);
            return printingConfig;
        }

        public PrintingConfig<TOwner> IgnoreProperty()
        {
            serializerSettings.membersToIgnor.Add(propertyInfo);
            return ParentConfig;
        }
    }
}
