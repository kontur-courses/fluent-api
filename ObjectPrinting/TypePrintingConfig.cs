using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropType> : IMemberPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }
        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            printingConfig.AddTypeToBeAlternativelySerialized(typeof(TPropType), print);

            return printingConfig;
        }
        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            if (!typeof(TPropType).IsNumericType())
                throw new ArgumentException("Использованный тип не является допустимым.");

            printingConfig
                .AddNumericTypeToBeAlternativelySerializedUsingCultureInfo(typeof(TPropType), culture);
            return printingConfig;
        }


    }
}
