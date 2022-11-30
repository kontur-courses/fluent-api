using System;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLength)
        {
            if (propConfig is null)
            {
                throw new ArgumentNullException(nameof(propConfig));
            }

            if (maxLength < 0)
            {
                throw new ArgumentException(nameof(maxLength));
            }

            propConfig.Using(x => x.Substring(0, Math.Min(maxLength, x.Length)));
            
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }
    }
}