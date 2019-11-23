using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingExtension
    {
        
        public static PrintingConfig<TOwner> TrimToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> obj, int i)
        {
            throw new NotImplementedException();
        }
        public static PrintingConfig<TOwner> SetCulture<TOwner>(this PropertyPrintingConfig<TOwner, int> obj,
            CultureInfo info)
        {
            throw new NotImplementedException();
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this PropertyPrintingConfig<TOwner, double> obj,
            CultureInfo info)
        {
            throw new NotImplementedException();
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this PropertyPrintingConfig<TOwner, byte> obj,
            CultureInfo info)
        {
            throw new NotImplementedException();
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this PropertyPrintingConfig<TOwner, float> obj,
            CultureInfo info)
        {
            return (obj as IPropertyPrintingConfig<TOwner>).Config;
        }
    }
}