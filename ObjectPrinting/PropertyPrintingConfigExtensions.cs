using System;

namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> TrimTo<TOwner>(this PropertyPrintingConfig<TOwner, string> config, int maxLength)
    {
        Func<string, string> serializer = str =>
        {
            if (str == null) { return "null";}

            string result;
            if (str.Length > maxLength)
            {
                result = str.Substring(0, maxLength);
            }
            else
            {
                result = str; 
            }

            return result;
        
        };

        return config.Using(serializer);
    }
}