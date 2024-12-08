using System;
using System.Reflection;

namespace ObjectPrinting.Configs
{
    public class PropertyPrintingConfig<TOwner, TPropType>(
        PrintingConfig<TOwner> printingConfig,
        MemberInfo? memberInfo = null)
    {
        public PrintingConfig<TOwner> PrintingConfig => printingConfig;
        public MemberInfo? MemberInfo => memberInfo;

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (memberInfo is null)
            {
                printingConfig.AddSpecialTypeProcessing(print);
            }
            else
            {
                printingConfig.AddSpecialProperyProcessing(print, memberInfo);

            }
            return printingConfig;
        }
    }
}