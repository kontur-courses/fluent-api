using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.PrintingConfigs
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo member; 

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo member = null)
        {
            this.printingConfig = printingConfig;
            this.member = member;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (member == null)
                printingConfig.SetTypeSerializer(typeof(TPropType), print);
            else
                printingConfig.SetMemberSerializer(member, print);

            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            return Using(value => value is IFormattable formattable
                ? formattable.ToString(null, culture)
                : value?.ToString());
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        MemberInfo IPropertyPrintingConfig<TOwner, TPropType>.Member => member;
    }
}