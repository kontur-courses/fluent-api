using ObjectPrinting.PropertyPrintingConfig;

namespace ObjectPrinting.Extensions;

public static class PropertyPrintingConfigExtensions
{
    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        return config(ObjectPrinter.For<T>()).PrintToString(obj);
    }

    public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
        this PropertyPrintingConfig<TOwner, TPropType> propConfig, CultureInfo culture, string? format = null)
        where TPropType : IFormattable
    {
        var parentConfig = ((IPropertyPrintingConfig<TOwner, TPropType>)propConfig).ParentConfig;

        parentConfig.TypeSerializationMethod.AddMethod(typeof(TPropType), (Func<TPropType, string>)FormatFunc);

        return parentConfig;

        string FormatFunc(TPropType obj) => obj.ToString(format, culture);
    }

    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig,
        int maxLen)
    {
        return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
    }
}