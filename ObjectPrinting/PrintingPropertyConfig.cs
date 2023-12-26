using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingPropertyConfig<TOwner, TProperty>
    {
        private readonly MemberInfo memberInfo;
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Dictionary<MemberInfo, Func<object, string>> serializers;

        public PrintingPropertyConfig(PrintingConfig<TOwner> printingConfig,
            Dictionary<MemberInfo, Func<object, string>> serializers,
            MemberInfo memberInfo)
        {
            this.printingConfig = printingConfig;
            this.serializers = serializers;
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> To(Func<TProperty, string> serializeRule)
        {
            serializers[memberInfo] = x => serializeRule((TProperty)x);
            return printingConfig;
        }
    }
}
