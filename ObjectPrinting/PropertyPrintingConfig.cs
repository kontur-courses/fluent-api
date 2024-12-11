using System;
using System.Globalization;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> printingConfig;
    protected internal readonly string? PropertyName;

    public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string? propertyName = null)
    {
        this.printingConfig = printingConfig;
        PropertyName = propertyName;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        if (string.IsNullOrEmpty(PropertyName))
        {
            printingConfig.AddSerializationMethod(print);
        }
        else
        {
            printingConfig.AddSerializationMethod(print, PropertyName);
        }
        
        return ParentConfig;
    }

    public PrintingConfig<TOwner> Using(CultureInfo culture)
    {
        if (string.IsNullOrEmpty(PropertyName))
        {
            printingConfig.SpecifyTheCulture<TPropType>(culture);
        }
        else
        {
            printingConfig.SpecifyTheCulture(culture, PropertyName);
        }

        return ParentConfig;
    }

    public PrintingConfig<TOwner> ParentConfig => printingConfig;
}