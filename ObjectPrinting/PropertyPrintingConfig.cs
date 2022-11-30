using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly MemberInfo memberInfo;
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Dictionary<MemberInfo, Func<object, string>> serializers;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
            Dictionary<MemberInfo, Func<object, string>> serializers,
            MemberInfo memberInfo)
        {
            this.printingConfig = printingConfig;
            this.serializers = serializers;
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            serializers[memberInfo] = x => print((TPropType)x);
            return printingConfig;
        }
    }
}