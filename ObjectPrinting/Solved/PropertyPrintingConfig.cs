using System;
using System.Globalization;

namespace ObjectPrinting.Solved
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> _printingConfig;
        private Func<TPropType, string> _printSettings;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            _printingConfig = printingConfig;
            _printSettings = null;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            _printSettings = print;
            return _printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            _printSettings = x => string.Format(culture, "{0}", x);
            return _printingConfig;
        }

        public string UseSettings(object property)
        {
            return _printSettings((TPropType) property);
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => _printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}