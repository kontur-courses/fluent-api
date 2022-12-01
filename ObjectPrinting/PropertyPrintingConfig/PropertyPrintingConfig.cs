using System;
using System.Reflection;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.PropertyPrintingConfig
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        internal readonly MemberInfo MemberInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo memberInfo = null)
        {
            this.printingConfig = printingConfig;
            MemberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (MemberInfo is null)
                printingConfig.TypeSerializationRule.AddRule(typeof(TPropType), print);
            else
                printingConfig.MemberSerializationRule.AddRule(MemberInfo, print);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

}