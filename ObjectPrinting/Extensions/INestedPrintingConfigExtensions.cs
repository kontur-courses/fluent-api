using System;
using System.Globalization;

namespace ObjectPrinting.Extensions
{
    public static class INestedPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner,TMember>(this INestedPrintingConfig<TOwner, 
            TMember> config, CultureInfo culture, string format = null)
        where TMember: IFormattable
        {
            return config.Using(item => item.ToString(format, culture));
        }
    }
}