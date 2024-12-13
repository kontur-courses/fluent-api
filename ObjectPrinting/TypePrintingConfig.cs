using System;
using System.Globalization;
using ObjectPrinting;

public class TypePrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> parentConfig;
    private readonly Type type;

    public TypePrintingConfig(PrintingConfig<TOwner> parentConfig, Type type)
    {
        this.parentConfig = parentConfig;
        this.type = type;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        parentConfig.AddTypeSerializer<TPropType>(obj => print((TPropType)obj));
        return parentConfig;
    }

    public PrintingConfig<TOwner> WithCulture(CultureInfo culture)
    {
        parentConfig.AddTypeCulture<TPropType>(culture);
        return parentConfig;
    }

    public PrintingConfig<TOwner> Exclude()
    {
        parentConfig.AddExcludedType(type);
        return parentConfig;
    }
}