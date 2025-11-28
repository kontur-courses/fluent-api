using System;

namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> TrimmedTo<TOwner>(
        this IPropertyPrintingConfig<TOwner, string> config, 
        int length)
    {
        return ImplementTrimmedTo(config, length);
    }
    
    private static PrintingConfig<TOwner> ImplementTrimmedTo<TOwner, TString>(
        IPropertyPrintingConfig<TOwner, TString> config, 
        int length)
    {
        if (length < 0)
            throw new ArgumentException("Length must be non-negative", nameof(length));
            
        return config.Using(str => 
        {
            if (str == null) 
                return "null";
                
            var stringValue = str.ToString();
            if (stringValue == null || stringValue.Length <= length) 
                return stringValue ?? "null";
                
            return stringValue[..length];
        });
    }
}