using System;
using System.Globalization;
using System.Linq;

namespace ObjectPrinting.Extensions
{
    public static class InnerPrintingConfigExtensions
    { 
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this IInnerPrintingConfig<TOwner, string> typeConfig, int maxLen)
        {
            typeConfig.ParentConfig.TypeSerializers[typeof(string)] = obj => ((string) obj).Truncate(maxLen);
            return typeConfig.ParentConfig;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this IInnerPrintingConfig<TOwner, double> typeConfig, CultureInfo culture)
        {
            typeConfig.ParentConfig.TypeSerializers[typeof(double)] = obj => ((double) obj).ToString(culture);
            return typeConfig.ParentConfig;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this IInnerPrintingConfig<TOwner, DateTime> typeConfig, CultureInfo culture)
        {
            typeConfig.ParentConfig.TypeSerializers[typeof(DateTime)] = obj => ((DateTime) obj).ToString(culture);
            return typeConfig.ParentConfig;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(this IInnerPrintingConfig<TOwner, float> typeConfig, CultureInfo culture)
        {
            typeConfig.ParentConfig.TypeSerializers[typeof(float)] = obj => ((float) obj).ToString(culture);
            return typeConfig.ParentConfig;
        }
    }
}