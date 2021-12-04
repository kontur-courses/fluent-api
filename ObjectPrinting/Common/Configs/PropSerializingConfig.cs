using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting.Common.Configs
{
    public class PropSerializingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Dictionary<MemberInfo, Func<object, string>> serializers;
        private readonly MemberInfo memberInfo;

        public PropSerializingConfig(PrintingConfig<TOwner> printingConfig, Dictionary<MemberInfo, Func<object, string>> serializers,
            MemberInfo memberInfo)
        {
            this.printingConfig = printingConfig;
            this.serializers = serializers;
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
        {
            serializers[memberInfo] = x => serializer((TPropType) x);
            return printingConfig;
        }
    }
}