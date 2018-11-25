using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting
{
    public class MemberPrintingConfig<TOwner, TPropType>
    {
        private readonly Dictionary<MemberInfo, Func<object, string>> serializationMemberMap;
        private readonly MemberInfo memberInfo;
        private readonly PrintingConfig<TOwner> printingConfig;

        public MemberPrintingConfig(Dictionary<MemberInfo, Func<object, string>> serializationMemberMap, MemberInfo memberInfo, PrintingConfig<TOwner> printingConfig)
        {
            this.serializationMemberMap = serializationMemberMap;
            this.memberInfo = memberInfo;
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> func)
        {
            serializationMemberMap[memberInfo] = arg => func((TPropType)arg);
            return printingConfig;
        }
        
    }

    public static class MemberPrintingConfigExtension
    {
        public static PrintingConfig<TOwner> Cut<TOwner>(this MemberPrintingConfig<TOwner, string> config, int count)
        {
            return config.Using(str => str.Substring(0, count));
        }
    }
}