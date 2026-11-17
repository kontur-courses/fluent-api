using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.Solved
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo? memberInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo? memberInfo = null)
        {
            this.printingConfig = printingConfig;
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            printingConfig.SetSerializer(print, memberInfo);

            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}