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
            var interfacedPropConfig = (IPropertyPrintingConfig<TOwner, string>)propConfig;
            interfacedPropConfig.SpecialPrintingFunctionsForProperties[interfacedPropConfig.Property] =
                o => ((string)o).Substring(0, ((string)o).Length < maxLen ? ((string)o).Length : maxLen);
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig,
            CultureInfo culture)
        {
            var interfacedPropConfig = (IPropertyPrintingConfig<TOwner, double>) propConfig;
            interfacedPropConfig.SpecialPrintingFunctionsForTypes[typeof(double)] = o => ((double) o).ToString(culture);
            return interfacedPropConfig.ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig,
            CultureInfo culture)
        {
            var interfacedPropConfig = (IPropertyPrintingConfig<TOwner, int>)propConfig;
            interfacedPropConfig.SpecialPrintingFunctionsForTypes[typeof(int)] = o => ((int)o).ToString(culture);
            return interfacedPropConfig.ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, sbyte> propConfig,
            CultureInfo culture)
        {
            var interfacedPropConfig = (IPropertyPrintingConfig<TOwner, sbyte>)propConfig;
            interfacedPropConfig.SpecialPrintingFunctionsForTypes[typeof(sbyte)] = o => ((sbyte)o).ToString(culture);
            return interfacedPropConfig.ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, byte> propConfig,
                    CultureInfo culture)
        {
            var interfacedPropConfig = (IPropertyPrintingConfig<TOwner, byte>)propConfig;
            interfacedPropConfig.SpecialPrintingFunctionsForTypes[typeof(byte)] = o => ((byte)o).ToString(culture);
            return interfacedPropConfig.ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, short> propConfig,
                    CultureInfo culture)
        {
            var interfacedPropConfig = (IPropertyPrintingConfig<TOwner, short>)propConfig;
            interfacedPropConfig.SpecialPrintingFunctionsForTypes[typeof(short)] = o => ((short)o).ToString(culture);
            return interfacedPropConfig.ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, ushort> propConfig,
                    CultureInfo culture)
        {
            var interfacedPropConfig = (IPropertyPrintingConfig<TOwner, ushort>)propConfig;
            interfacedPropConfig.SpecialPrintingFunctionsForTypes[typeof(ushort)] = o => ((ushort)o).ToString(culture);
            return interfacedPropConfig.ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, uint> propConfig,
                    CultureInfo culture)
        {
            var interfacedPropConfig = (IPropertyPrintingConfig<TOwner, uint>)propConfig;
            interfacedPropConfig.SpecialPrintingFunctionsForTypes[typeof(uint)] = o => ((uint)o).ToString(culture);
            return interfacedPropConfig.ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> propConfig,
                    CultureInfo culture)
        {
            var interfacedPropConfig = (IPropertyPrintingConfig<TOwner, long>)propConfig;
            interfacedPropConfig.SpecialPrintingFunctionsForTypes[typeof(long)] = o => ((long)o).ToString(culture);
            return interfacedPropConfig.ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, ulong> propConfig,
                    CultureInfo culture)
        {
            var interfacedPropConfig = (IPropertyPrintingConfig<TOwner, ulong>)propConfig;
            interfacedPropConfig.SpecialPrintingFunctionsForTypes[typeof(ulong)] = o => ((ulong)o).ToString(culture);
            return interfacedPropConfig.ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propConfig,
                    CultureInfo culture)
        {
            var interfacedPropConfig = (IPropertyPrintingConfig<TOwner, float>)propConfig;
            interfacedPropConfig.SpecialPrintingFunctionsForTypes[typeof(float)] = o => ((float)o).ToString(culture);
            return interfacedPropConfig.ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, decimal> propConfig,
                    CultureInfo culture)
        {
            var interfacedPropConfig = (IPropertyPrintingConfig<TOwner, decimal>)propConfig;
            interfacedPropConfig.SpecialPrintingFunctionsForTypes[typeof(decimal)] = o => ((decimal)o).ToString(culture);
            return interfacedPropConfig.ParentConfig;
        }
    }
}