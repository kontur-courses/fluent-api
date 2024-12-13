using System;
using System.Globalization;
using ObjectPrinting.PrintingConfig;

namespace ObjectPrinting.Solved
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyConfigMember<TOwner, string> propConfig, int maxLen)
        {
            propConfig.ParentConfig.DataConfig.PropertyTrim.Add(propConfig.MemberInfo, maxLen);

            return propConfig.ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner, TProType>(
            this IPropertyPrintingConfig<TOwner, TProType> propertyPrintingConfig,
            CultureInfo cultureInfo) where TProType : struct, IFormattable

        {
            propertyPrintingConfig.ParentConfig.DataConfig.TypeCultures.Add(typeof(TProType), cultureInfo);

            return propertyPrintingConfig.ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner, TProType>(
            this IPropertyPrintingConfig<TOwner, TProType> propertyPrintingConfig,
            Func<string, string> print) where TProType : struct, IFormattable

        {
            Func<object, string> func = obj => print((string)obj);

            propertyPrintingConfig.ParentConfig.DataConfig.TypeSerializers.Add(typeof(TProType), func);
            return propertyPrintingConfig.ParentConfig;
        }
        
    }
}