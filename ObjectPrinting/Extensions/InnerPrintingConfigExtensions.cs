using System;
using System.Globalization;
using ObjectPrinting.InnerPrintingConfig;

namespace ObjectPrinting.Extensions
{
    public static class InnerPrintingConfigExtensions
    {
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