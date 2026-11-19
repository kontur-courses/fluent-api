using System;
using System.Globalization;

namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> Using<TOwner, TPropertyType>(this PropertyPrintingConfig<TOwner, TPropertyType> propertyPrintingConfig, CultureInfo cultureInfo)
        where TPropertyType : IFormattable
    {
        throw  new NotImplementedException();
    }
    
    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propertyPrintingConfig, int length)
    {
        throw  new NotImplementedException();
    }
}