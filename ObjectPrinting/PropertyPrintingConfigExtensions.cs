using System.Globalization;
using System.Net;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertyPrintingConfig<TOwner, int> propertyPrintingConfig, CultureInfo cultureInfo)
        {
            return ((IPropertyPrintingConfig<TOwner, int>) propertyPrintingConfig)
                   .PrintingConfig;
        }
    }
}