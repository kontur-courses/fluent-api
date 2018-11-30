using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtension
    {        
        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner, TPropType> config,
            CultureInfo cultureInfo)
        {
            return ((IPropertyPrintingConfig<TOwner>) config).ParentConfig;
        }
        
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> config,
            int length)
        {
            return ((IPropertyPrintingConfig<TOwner>) config).ParentConfig;
        }
    }
}