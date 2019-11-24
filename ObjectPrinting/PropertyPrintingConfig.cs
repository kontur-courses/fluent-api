using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Config config;
        private readonly MemberInfo member;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Config config, MemberInfo member)
        {
            this.printingConfig = printingConfig;
            this.config = config;
            this.member = member;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            config.Printing[member] = obj => print((TPropType)obj);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        Config IPropertyPrintingConfig<TOwner, TPropType>.Config => config;
        MemberInfo IPropertyPrintingConfig<TOwner, TPropType>.Member => member;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        Config Config { get; }
        MemberInfo Member { get; }
    }
}