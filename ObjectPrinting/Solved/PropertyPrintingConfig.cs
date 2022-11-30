using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.Solved
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> _printingConfig;
        private readonly PropertyInfo _propertyInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            _printingConfig = printingConfig;
            _propertyInfo = null;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo)
        {
            _printingConfig = printingConfig;
            _propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> printSetting)
        {
            if (_propertyInfo is null)
                _printingConfig.AddPrintSetting(typeof(TPropType), printSetting);
            else
                _printingConfig.AddPrintSetting(_propertyInfo.Name, printSetting);
            return _printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            Func<TPropType, string> printSetting = x => string.Format(culture, "{0}", x);
            return Using(printSetting);
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => _printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        public PrintingConfig<TOwner> Using(Func<TPropType, string> print);
        public PrintingConfig<TOwner> Using(CultureInfo culture);
    }
}