using System;
using System.Globalization;
using ObjectPrinting.InnerPrintingConfigs;

namespace ObjectPrinting.Extensions
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, double> config, CultureInfo culture)
        {
            return config.Using(x => x.ToString(culture));;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, float> config, CultureInfo culture)
        {
            return config.Using(x => x.ToString(culture));;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, DateTime> config, CultureInfo culture)
        {
            return config.Using(x => x.ToString(culture));;
        }
    }
}