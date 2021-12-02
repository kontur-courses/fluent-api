using System;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.Common.Configs
{
    public class SerializingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly IDictionary serializers;
        private readonly MemberInfo memberInfo;

        public SerializingConfig(PrintingConfig<TOwner> printingConfig, IDictionary serializers)
        {
            this.printingConfig = printingConfig;
            this.serializers = serializers;
        }

        public SerializingConfig(PrintingConfig<TOwner> printingConfig, IDictionary serializers,
            MemberInfo memberInfo)
        {
            this.printingConfig = printingConfig;
            this.serializers = serializers;
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
        {
            serializers.Add(memberInfo == null ? typeof(TPropType) : memberInfo, serializer);
            return printingConfig;
        }
    }
}