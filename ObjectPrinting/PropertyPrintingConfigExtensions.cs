using System;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLength)
        {
            propConfig.Using(prop => prop[..Math.Min(maxLength, prop.Length)]);
            return propConfig.ParentConfig;
        }
    }
}
