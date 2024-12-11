using System.Globalization;

namespace ObjectPrintingHomework;

public class PropertyPrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> parentConfig;
    private readonly string propertyName;

    public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, string propertyName = null)
    {
        this.parentConfig = parentConfig;
        this.propertyName = propertyName;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
    {
        if (propertyName == null)
            parentConfig.AddTypeSerializer(serializer);
        else
            parentConfig.AddPropertySerializer(propertyName, obj => serializer((TPropType)obj));

        return parentConfig;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, CultureInfo> culture)
    {
        parentConfig.SetCulture<TPropType>(culture(default!));
        return parentConfig;
    }

    public PrintingConfig<TOwner> TrimmedToLength(int startIndex, int maxLength)
    {
        if (typeof(TPropType) != typeof(string))
            throw new InvalidOperationException("Trimming is only supported for string properties");

        if (propertyName == null)
            throw new InvalidOperationException("Property name must be specified for trimming");

        parentConfig.SetStringPropertyLength(propertyName, startIndex, maxLength);
        return parentConfig;
    }
}