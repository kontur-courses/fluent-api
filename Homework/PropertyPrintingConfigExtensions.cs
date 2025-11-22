using System.Globalization;

namespace Homework;

public static class PropertyPrintingConfigExtensions
{
    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        return config(ObjectPrinter.For<T>()).PrintToString(obj);
    }

    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string?> propConfig, int maxLen)
    {
        var configIface = (IPropertyPrintingConfig<TOwner, string>)propConfig;
        if (configIface.MemberInfo is not null)
            ((IPrintingConfig)configIface.ParentConfig).StringsTrim[configIface.MemberInfo] = maxLen;

        return configIface.ParentConfig;
    }

    public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
        this PropertyPrintingConfig<TOwner, TPropType> propConfig,
        CultureInfo culture)
            where TPropType : IFormattable
    {
        var configIface = (IPropertyPrintingConfig<TOwner, TPropType>)propConfig;
        ((IPrintingConfig)configIface.ParentConfig).TypesCultureInfo[typeof(TPropType)] = culture;

        return configIface.ParentConfig;
    }
}