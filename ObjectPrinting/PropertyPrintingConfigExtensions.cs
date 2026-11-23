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

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            if (propConfig.FieldMember == null)
            {
                throw new InvalidOperationException("TrimmedToLength можно применить только к конкретному свойству");
            }
            if (maxLen < 0)
            {
                throw new ArgumentException("Длина не может быть отрицательной");
            }

            propConfig.ParentConfig.AddNewFieldTrim(propConfig.FieldMember.DeclaringType!, propConfig.FieldMember.Name, maxLen);

            return propConfig.ParentConfig;
        }

        public static PrintingConfig<TOwner> UsingCulture<TOwner, TPropType>(this PropertyPrintingConfig<TOwner, TPropType> propConfig, CultureInfo culture) where TPropType : IFormattable
        {
            if (propConfig.FieldMember != null)
            {
                throw new InvalidOperationException("UsingCulture нельзя применить к конкретному свойству");
            }

            propConfig.ParentConfig.AddNewTypeCultureInfo(typeof(TPropType), culture);
            return propConfig.ParentConfig;
        }

    }
}