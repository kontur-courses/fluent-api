using System;
using System.Globalization;
using ObjectPrinting.InnerPrintingConfigs;

namespace ObjectPrinting.Extensions
{
    public static class MemberPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this MemberPrintingConfig<TOwner, double> config, CultureInfo culture)
        {
            return config.Using(x => x.ToString(culture));;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this MemberPrintingConfig<TOwner, float> config, CultureInfo culture)
        {
            return config.Using(x => x.ToString(culture));;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this MemberPrintingConfig<TOwner, DateTime> config, CultureInfo culture)
        {
            return config.Using(x => x.ToString(culture));;
        }
    }
}