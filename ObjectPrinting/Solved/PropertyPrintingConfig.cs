using System;
using System.Globalization;

namespace ObjectPrinting.Solved
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Action<Func<TPropType, string>>? registerPrinter;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        internal PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
            Action<Func<TPropType, string>> registerPrinter) : this(printingConfig)
        {
            this.registerPrinter = registerPrinter;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            registerPrinter?.Invoke(print);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            if (!typeof(IFormattable).IsAssignableFrom(typeof(TPropType)))
                return printingConfig;

            return Using(value => ((IFormattable)value!).ToString(null, culture));
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}
