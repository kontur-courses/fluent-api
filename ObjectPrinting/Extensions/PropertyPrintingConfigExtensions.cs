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

        parentConfig.TypeSerializationMethod.Add(typeof(TPropType), obj => ((TPropType)obj).ToString(format, culture));

        return parentConfig;
    }

    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig,
        int maxLength)
    {
        if (maxLength < 1)
            throw new ArgumentException($"{nameof(maxLength)} should be greater than 1");

        var parentConfig = ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        
        if (propConfig.PropertyMemberInfo is null) return parentConfig;

        parentConfig.MemberSerializationMethod.Add(propConfig.PropertyMemberInfo, str =>
        {
            var stringValue = (string)str;
            return stringValue.Length <= maxLength
                ? stringValue
                : stringValue[..maxLength];
        });

        return parentConfig;
    }
}