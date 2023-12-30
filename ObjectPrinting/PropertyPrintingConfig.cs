using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TMemberType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo memberInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo memberInfo)
        {
            this.printingConfig = printingConfig;
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TMemberType, string> print)
        {
            if (memberInfo != null)
            {
                printingConfig.AddSerializeMember(print, memberInfo);
            }
            else
            {
                printingConfig.AddSerializeByType(typeof(TMemberType), print);
            }

            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            return Using(x =>((IConvertible)x).ToString(culture));
        }
    }
}