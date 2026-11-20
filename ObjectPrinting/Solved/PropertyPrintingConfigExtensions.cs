using System;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace ObjectPrinting.Solved
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            var printingConfig = ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
            return propConfig.Using(s => s.Length < maxLen ? s : s[..maxLen]);
        }

        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner, TPropType> propertyPrintingConfig,
            CultureInfo culture)
            where TPropType : INumber<TPropType>
        {
            var printingConfig = ((IPropertyPrintingConfig<TOwner, TPropType>)propertyPrintingConfig).ParentConfig;
            var propertiesOfPropType = typeof(TOwner)
                .GetProperties(PrintingConfig<TOwner>.PublicInstanceFlag)
                .Where(m => m.PropertyType == typeof(TPropType));
            var fieldsOfPropType = typeof(TOwner)
                .GetFields(PrintingConfig<TOwner>.PublicInstanceFlag)
                .Where(m => m.FieldType == typeof(TPropType));
            foreach (var property in propertiesOfPropType)
            {
                printingConfig.CustomSerializers[property] = value => ((TPropType)value).ToString(null, culture);
            }
            foreach (var field in fieldsOfPropType)
            {
                printingConfig.CustomSerializers[field] = value => ((TPropType)value).ToString(null, culture);
            }
            return printingConfig;
        }

    }
}