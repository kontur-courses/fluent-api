using System;

namespace ObjectPrinting.Extensions
{
    public static class ConcretePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this ConcretePropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            if (maxLen < 0)
                throw new ArgumentException($"{nameof(maxLen)} {maxLen} was negative");

            var parentConfig = (IPrintingConfig) ((IPropertyPrintingConfig<TOwner>) propConfig).ParentConfig;
            var propertyInfo = ((IConcretePropertyPrintingConfig<TOwner>) propConfig).PropertyInfo;
            var settings = parentConfig.Settings;

            return new PrintingConfig<TOwner>(settings.AddMaxLengthOfProperty(propertyInfo, maxLen));
        }
    }
}