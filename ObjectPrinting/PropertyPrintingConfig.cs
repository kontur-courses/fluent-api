using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> printingConfig;
    protected internal readonly PropertyInfo? Property;

    public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo? property = null)
    {
        this.printingConfig = printingConfig;
        Property = property;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        if (Property is null)
        {
            printingConfig.AddSerializationMethod(print);
        }
        else
        {
            printingConfig.AddSerializationMethod(print, Property);
        }
        
        return ParentConfig;
    }

    public PrintingConfig<TOwner> Using(CultureInfo culture)
    {
        if (Property is null)
        {
            printingConfig.SpecifyTheCulture<TPropType>(culture);
        }
        else
        {
            printingConfig.SpecifyTheCulture(culture, Property);
        }

        return ParentConfig;
    }

    public PrintingConfig<TOwner> ParentConfig => printingConfig;
}