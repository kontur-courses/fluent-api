using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }
        
        public static PrintingConfig<TOwner> Using<TOwner,TType>(
            this PrintingConfig<TOwner>.PropertyPrintingConfig<TOwner, TType> config,
            CultureInfo cultureInfo) where TType: IFormattable
        {
            
            ((PrintingConfig<TOwner>.IPropertyPrintingConfig<TOwner>)config).SetCulture<TType>(cultureInfo);
            return ((PrintingConfig<TOwner>.IPropertyPrintingConfig<TOwner>)config).ParentConfig;
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PrintingConfig<TOwner>.PropertyPrintingConfig<TOwner, string> propConfig, int lengthLimit)
        {
            propConfig.Using(s => s.Substring(0, Math.Min(s.Length, lengthLimit)));
            return ((PrintingConfig<TOwner>.IPropertyPrintingConfig<TOwner>)propConfig).ParentConfig;
        }
    }
}