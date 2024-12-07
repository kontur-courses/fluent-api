using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingPropertyConfig<TOwner, TProperty>
    {
        private readonly MemberInfo memberInfo;
        private int nestingLevel;
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Dictionary<(MemberInfo, int), Func<object, string>> serializers;

        public PrintingPropertyConfig(PrintingConfig<TOwner> printingConfig,
            Dictionary<(MemberInfo, int), Func<object, string>> serializers,
            MemberInfo memberInfo, int nestingLevel = -1)
        {
            this.printingConfig = printingConfig;
            this.serializers = serializers;
            this.memberInfo = memberInfo;
            this.nestingLevel = nestingLevel;
        }

        public PrintingConfig<TOwner> To(Func<TProperty, string> serializeRule)
        {
            serializers[(memberInfo, nestingLevel)] = x => serializeRule((TProperty)x);
            return printingConfig;
        }
    }
}
