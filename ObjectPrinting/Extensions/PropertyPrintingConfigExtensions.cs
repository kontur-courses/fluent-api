using System;

namespace ObjectPrinting.Extensions;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this PropertyPrintingConfig<TOwner, string> propConfig, 
        int maxLen)
    {
        if (string.IsNullOrEmpty(propConfig.PropertyName))
            throw new ArgumentException("The name of the property is not specified.");
        
        propConfig.ParentConfig.AddLengthProperty(propConfig.PropertyName, maxLen);
        
        return propConfig.ParentConfig;
    }
    
    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        return config(ObjectPrinter.For<T>()).PrintToString(obj);
    }
}