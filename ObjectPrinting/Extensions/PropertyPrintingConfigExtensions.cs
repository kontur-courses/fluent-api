using System;
using ObjectPrinting.Configurations;

namespace ObjectPrinting.Extensions;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
    {
        return propConfig.Using(s => TrimmedToLength(s, maxLen));
    }

    private static string TrimmedToLength(string startValue, int maxLen)
    {
        if (string.IsNullOrEmpty(startValue) || startValue.Length <= maxLen)
        {
            return startValue;
        }
        return startValue[..maxLen];
    }
}