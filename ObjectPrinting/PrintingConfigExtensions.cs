using System;
using System.Globalization;
using ObjectPrinting.Interfaces;

namespace ObjectPrinting;

public static class PrintingConfigExtensions
{
    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        return config(ObjectPrinter.For<T>()).PrintToString(obj);
    }

    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this IPrintingConfig<TOwner, string> config, int maxLen)
    {
        return config.Using(x => x == null ? "null" : x[..Math.Min(x.Length, maxLen)]);
    }
    
    public static PrintingConfig<TOwner> SetCulture<TOwner, TPropType>(this IPrintingConfig<TOwner, TPropType> config, 
        CultureInfo culture) 
        where TPropType: IFormattable  
    {
        return config.Using(x => x.ToString(null, culture));
    }
}