using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Expression<Func<TOwner, TPropType>> memberSelector;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            this.memberSelector = memberSelector;
            this.printingConfig = printingConfig;
        }

        public void SetMaxLength(int maxLength)
        {
            printingConfig.SetMaxLengthRestriction(memberSelector, maxLength);
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> printMethod)
        {
            if (memberSelector == null)
            {
                printingConfig.SetCustomSerializationMethod(printMethod);
            }
            else
            {
                printingConfig.SetCustomSerializationMethod(printMethod, memberSelector);
            }

            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            printingConfig.SetCustomCulture<TPropType>(culture);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }
}