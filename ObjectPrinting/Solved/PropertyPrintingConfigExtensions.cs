using System;
using System.Globalization;

namespace ObjectPrinting.Solved
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this IPropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            if (propConfig is not PropertyConfigMember<TOwner, string> member)
            {
                throw new TypeAccessException("ты пытаешься все строки обрезать?? такое здесь не приветствуется пошл отсюда");
            }
           
            propConfig.ParentConfig.GetConfig.PropertyTrim.Add(member.MemberInfo, maxLen);

            return propConfig.ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner, TProType>(
            this IPropertyPrintingConfig<TOwner, TProType> propertyPrintingConfig,
            CultureInfo cultureInfo)

        {
            propertyPrintingConfig.ParentConfig.GetConfig.TypeCultures.Add(typeof(TProType), cultureInfo);

            return propertyPrintingConfig.ParentConfig;
        }
    }
}