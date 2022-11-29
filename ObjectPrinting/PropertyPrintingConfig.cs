using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProperty>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo memberInfo;
        
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }
        
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo memberInfo)
        {
            this.printingConfig = printingConfig;
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TProperty, string> func)
        {
            if (memberInfo != null)
                printingConfig.PropertySerializes.Add(memberInfo.Name, func);
            else
                printingConfig.TypeSerializes.Add(typeof(TProperty), func);

            return printingConfig;
        }
        
        public PrintingConfig<TOwner> Using(CultureInfo cultureInfo)
        {
            printingConfig.Cultures.Add(typeof(TProperty), cultureInfo);
            return printingConfig;
        }
    }
}