using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IChildPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        internal readonly MemberInfo Member;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo member)
        {
            Member = member;
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            printingConfig.SetSerializer(Member, o => print((TPropType)o));
            return printingConfig;
        }

        PrintingConfig<TOwner> IChildPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }
}