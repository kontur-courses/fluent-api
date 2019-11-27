using System;

namespace ObjectPrinting.Extensions
{
    public static class ConcretePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this ConcretePropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            if (maxLen < 0)
                throw new ArgumentOutOfRangeException($"{nameof(maxLen)} {maxLen} was negative");

            var parentConfig = (IPrintingConfig) ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
            parentConfig.MaxLengthsOfProperties[((IConcretePropertyPrintingConfig) (propConfig)).PropertyInfo] = maxLen;

            return ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
        }
    }
}