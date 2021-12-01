using System;
using System.Globalization;

namespace ObjectPrinting.Solved
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config, int maxNestingLevel)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj, maxNestingLevel);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
            Func<object, string> func = str => ((string)str).Substring(0,maxLen);
            parentConfig.AddFuncForProp(func);
            return parentConfig;
        }

        public static PrintingConfig<TOwner> UsingCulture<TOwner, TPropType>(this TypePrintingConfig<TOwner, TPropType> propConfig, CultureInfo cultureInfo)
            where TPropType: IFormattable
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, TPropType>)propConfig).ParentConfig;
            ((IPrintingConfig<TOwner>)parentConfig).SpecialCulture[typeof(TPropType)] = cultureInfo;
            return parentConfig;
        }

    }
}