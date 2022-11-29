using System.Globalization;

namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        return config(ObjectPrinter.For<T>()).PrintToString(obj);
    }

    public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
        this PropertyPrintingConfig<TOwner, TPropType> propConfig, CultureInfo culture)
        where TPropType : IConvertible
    {
        propConfig.ParentConfig.TypesCultures[typeof(TPropType)] = culture;
        return propConfig.ParentConfig;
    }

    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
    {
        if (maxLen < 0)
            throw new ArgumentException($"String length can't be less than zero, but {maxLen}");

        propConfig.ParentConfig.StringPropertyTrimIndex = maxLen;
        return propConfig.ParentConfig;
    }
}