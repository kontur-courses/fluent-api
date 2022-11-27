using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.PrintingConfig
{
    public static class PrintingConfigExtensions
    {
        public static PropertyPrintingConfig<TOwner, string> CutAfter<TOwner>(this PropertyPrintingConfig<TOwner, string> config, int number)
        {
            return new PropertyPrintingConfig<TOwner, string>(number, config);
        }
        
        public static TypePrintingConfig<TOwner, string> CutAfter<TOwner>(this TypePrintingConfig<TOwner, string> config, int number)
        {
            return new TypePrintingConfig<TOwner, string>(number, config);
        }

        public static TypePrintingConfig<TOwner, T> Using<TOwner, T>(this TypePrintingConfig<TOwner, T> config, CultureInfo cultureInfo) 
            where T : IFormattable 
        {
            return new TypePrintingConfig<TOwner, T>(typeof(T), cultureInfo, config);
        }
    }
}