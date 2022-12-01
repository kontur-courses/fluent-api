using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtension
    {
        public static PrintingConfig<TOwner> As<TOwner, T>(this TypePrintingConfig<TOwner, T> propConfig,
            CultureInfo cultureInfo)
            where T : IFormattable
        {
            propConfig.Parent.Cultures[typeof(T)] = cultureInfo;
            return propConfig.Parent;
        }
    }
}