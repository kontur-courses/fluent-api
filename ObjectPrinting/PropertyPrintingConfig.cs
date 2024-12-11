using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProperty>
    {
        private readonly PrintingConfig<TOwner> _config;
        private readonly PropertyInfo _propertyInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> config, PropertyInfo propertyInfo)
        {
            _config = config;
            _propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> UseSerializer(Func<TProperty, string> serializer)
        {
            _config.PropertySerializers[_propertyInfo] = value => serializer((TProperty)value);
            return _config;
        }

        public PrintingConfig<TOwner> SetMaxLength(int maxLength)
        {
            _config.PropertiesMaxLength[_propertyInfo] = maxLength;
            return _config;
        }
    }
}