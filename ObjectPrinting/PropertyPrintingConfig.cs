using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProperty>(PrintingConfig<TOwner> config, PropertyInfo propertyInfo)
    {
        public PrintingConfig<TOwner> UseSerializer(Func<TProperty, string> serializer)
        {
            config.PropertySerializers[propertyInfo] = value => serializer((TProperty)value);
            return config;
        }

        public PrintingConfig<TOwner> SetMaxLength(int maxLength)
        {
            config.PropertiesMaxLength[propertyInfo] = maxLength;
            return config;
        }
    }
}