using System;
using System.Globalization;
using ObjectPrinting.PrintingConfig;

namespace ObjectPrinting
{
    public static class PropertyExtensions
    {
        public static PrintingConfig<TOwner> CutString<TOwner>(this IPropertyConfig<TOwner, string> config,
            int maxLength)
        {
            config.Printer.SetStringCut(config.PropertyExpression, maxLength);
            return config.Printer;
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner, T>(this IPropertyConfig<TOwner, T> config,
            CultureInfo info) where T : IFormattable
        {
            config.Printer.SetSerializeMethodForProperty(config.PropertyExpression, (f) => f.ToString("", info));
            return config.Printer;
        }
    }
}