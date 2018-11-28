using System;

namespace ObjectPrinting.Config.Property
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner> (this PropertyPrintingConfig<TOwner, string> propertyConfig, int maxLen)
        {
            var parentConfig = ((IPrintingConfig<TOwner, string>) propertyConfig).ParentConfig;
            Func<object, string> trimFunction = obj => ((string) obj).Substring(0, maxLen);
            parentConfig.OverridePropertyPrinting(propertyConfig.PropertyToChange, trimFunction);

            return parentConfig;
        }
    }
}
