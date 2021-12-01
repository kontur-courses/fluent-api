using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> _printingConfig;
        private readonly Expression<Func<TOwner, TPropType>> _memberSelector;
        private PropertyInfo _propInfo;
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            _printingConfig = printingConfig;
            _memberSelector = memberSelector;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propInfo)
        {
            _printingConfig = printingConfig;
            _propInfo = propInfo;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            _printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            Func<PropertyInfo, object, string> printWrapper = (p, o) =>
            {
                var value = p.GetValue(o);
                return print((TPropType)value);
            };
            _printingConfig.serWay[typeof(TPropType)] = printWrapper;
            return _printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            return _printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => _printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}