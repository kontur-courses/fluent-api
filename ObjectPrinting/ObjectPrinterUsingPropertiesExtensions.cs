using System;
using System.Globalization;

namespace ObjectPrinting;

public static class ObjectPrinterUsingPropertiesExtensions
{
    public static PrintingConfig<TOwner> SetCulture<TOwner>(this ObjectPrinterUsingProperties<TOwner, int> properties, CultureInfo cultureInfo)
    {
        properties.PrintingConfig.SerializationsByType[typeof(int)] = obj => ((int)obj).ToString(cultureInfo);
        return properties.PrintingConfig;
    }
    
    public static PrintingConfig<TOwner> SetCulture<TOwner>(this ObjectPrinterUsingProperties<TOwner, double> properties, CultureInfo cultureInfo)
    {
        properties.PrintingConfig.SerializationsByType[typeof(double)] = obj => ((double)obj).ToString(cultureInfo);
        return properties.PrintingConfig;
    }
    
    public static PrintingConfig<TOwner> SetCulture<TOwner>(this ObjectPrinterUsingProperties<TOwner, decimal> properties, CultureInfo cultureInfo)
    {
        properties.PrintingConfig.SerializationsByType[typeof(decimal)] = obj => ((decimal)obj).ToString(cultureInfo);
        return properties.PrintingConfig;
    }
    
    public static PrintingConfig<TOwner> Trim<TOwner>(this ObjectPrinterUsingProperties<TOwner, string> properties, int length)
    {
        properties.PrintingConfig.SerializationsByType[typeof(string)] = obj => ((string)obj).Substring(0, Math.Min(length, ((string)obj).Length));
        return properties.PrintingConfig;
    }
}