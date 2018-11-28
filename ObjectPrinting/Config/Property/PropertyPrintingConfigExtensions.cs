using System;

namespace ObjectPrinting.Config.Property
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner> (this PropertyPrintingConfig<TOwner, string> propertyConfig, int maxLen)
        {
            if (maxLen < 0)
                throw new ArgumentException("Trimming length can't be negative");

            var parentConfig = ((IPrintingConfig<TOwner, string>) propertyConfig).ParentConfig;
            Func<object, string> trimFunction = obj =>
            {
                var str = (string) obj;
                return ((string) obj).Substring(0, Math.Min(str.Length, maxLen));
            };
            parentConfig.OverridePropertyPrinting(propertyConfig.PropertyToChange, trimFunction);

            return parentConfig;
        }
    }
}
