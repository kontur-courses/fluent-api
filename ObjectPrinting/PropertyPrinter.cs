using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrinter<TOwner, TOwnerProperty>
    {
        private readonly PrintingConfig<TOwner> _printingConfig;
        private readonly PropertyInfo _propertyInfo;
        
        public PropertyPrinter(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo)
        {
            _printingConfig = printingConfig;
            _propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<PropertyInfo, string> printOptions)
        {
            if (_propertyInfo != null)
                _printingConfig.AddSerilizationOptions<TOwnerProperty>(_propertyInfo, printOptions);
            else
                _printingConfig.AddSerilizationOptions<TOwnerProperty>(printOptions);
            
            return _printingConfig;
        }
    }
}