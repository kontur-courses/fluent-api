using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class MemberPrintingConfig<TOwner, TPropType> : IMemberPrintingConfig<TOwner, TPropType>
    {
        private readonly MemberInfo memberInfo;
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Dictionary<MemberInfo, Func<object, string>> serializationMemberMap;

        public MemberPrintingConfig(Dictionary<MemberInfo, Func<object, string>> serializationMemberMap,
            MemberInfo memberInfo, PrintingConfig<TOwner> printingConfig)
        {
            this.serializationMemberMap = serializationMemberMap;
            this.memberInfo = memberInfo;
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> format)
        {
            serializationMemberMap[memberInfo] = objectToPrint => format((TPropType) objectToPrint);
            return printingConfig;
        }
    }
}