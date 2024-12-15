using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropertyType>
    {
        private readonly PrintingConfig<TOwner> _config;

        public TypePrintingConfig(PrintingConfig<TOwner> config)
        {
            _config = config;
        }

        public PrintingConfig<TOwner> UseSerializer(Func<TPropertyType, string> serializer)
        {
            _config.TypeSerializers[typeof(TPropertyType)] = value => serializer((TPropertyType)value);
            return _config;
        }

        public PrintingConfig<TOwner> SetCulture(CultureInfo culture)
        {
            _config.Cultures[typeof(TPropertyType)] = culture;
            return _config;
        }
    }
}