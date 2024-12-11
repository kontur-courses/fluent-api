using System;
using System.Linq.Expressions;

namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        return config(ObjectPrinter.For<T>()).PrintToString(obj);
    }
    
    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
    {
        if (maxLen < 0)
            throw new ArgumentOutOfRangeException(nameof(maxLen), "Max length must be non-negative");

        IPropertyPrintingConfig<TOwner, string> propertyConfig = propConfig;
        var printingConfig = propertyConfig.ParentConfig;
        
        var memberSelector = propConfig.MemberSelector;

        var propertyName = ((MemberExpression)memberSelector.Body).Member.Name;

        printingConfig.AddPropertySerializer(propertyName, value =>
        {
            var stringValue = value as string ?? string.Empty;
            return stringValue.Length > maxLen ? stringValue[..maxLen] : stringValue;
        });

        return printingConfig;
    }
}