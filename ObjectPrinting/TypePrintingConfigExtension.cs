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
            if (propConfig.Parent.AlternativeCulturesForTypes.ContainsKey(typeof(T)))
                throw new InvalidOperationException("The culture for this type has already been set");
            propConfig.Parent.AlternativeCulturesForTypes[typeof(T)] = cultureInfo;
            return propConfig.Parent;
        }
    }
}