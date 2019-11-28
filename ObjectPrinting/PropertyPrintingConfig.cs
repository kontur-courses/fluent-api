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
            printingConfig.AddMaxLengthRestriction(memberSelector, maxLength);
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> printMethod)
        {
            if (memberSelector == null)
            {
                printingConfig.AddCustomSerializationMethod(printMethod);
            }
            else
            {
                printingConfig.AddCustomSerializationMethod(memberSelector, printMethod);
            }

            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            printingConfig.AddCustomCulture<TPropType>(culture);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}