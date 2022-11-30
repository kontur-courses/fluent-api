using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyExtensions
    {
        public static IPropertyConfig<TOwner,string> CutString<TOwner>(this IPropertyConfig<TOwner, string> config, int maxLength)
        {
            config.Printer.SetStringCut(config.PropertyExpression, maxLength);
            return config;
        }
    }
}