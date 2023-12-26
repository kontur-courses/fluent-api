using System;
using System.Globalization;
using ObjectPrinting.InnerPrintingConfigs;

namespace ObjectPrinting.Extensions
{
    public static class InnerPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this IInnerPrintingConfig<TOwner, double> config, CultureInfo culture)
        {
            return config.Using(x => x.ToString(culture));;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this IInnerPrintingConfig<TOwner, float> config, CultureInfo culture)
        {
            return config.Using(x => x.ToString(culture));;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this IInnerPrintingConfig<TOwner, DateTime> config, CultureInfo culture)
        {
            return config.Using(x => x.ToString(culture));;
        }
    }
}