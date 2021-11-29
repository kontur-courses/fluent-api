using System;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner,
            string> propConfig, int maxLen)
        {
            if (maxLen < 0)
                throw new ArgumentException($"Length can't be less than 0 but was {maxLen}");

            propConfig.Using(x => x[..Math.Min(x.Length, maxLen)]);
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }
    }
}