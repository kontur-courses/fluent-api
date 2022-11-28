using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting.Solved
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> _printingConfig;
        public Func<TPropType, string> AlternativePrint { get; set; }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            _printingConfig = printingConfig;
            AlternativePrint = null;
        }

        public string AlternativePrintInvoke(object prop)
        {
            return AlternativePrint((TPropType)prop);
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            AlternativePrint = print;
            return _printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            AlternativePrint = property => string.Format(culture, "{0}", property);
            return _printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => _printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}