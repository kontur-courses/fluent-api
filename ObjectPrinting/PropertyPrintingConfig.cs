using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType> : PrintingConfig<TOwner>, IPropertyPrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> printingConfig;
    public readonly Expression<Func<TOwner, TPropType>>? MemberSelector;

    public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
        Expression<Func<TOwner, TPropType>>? memberSelector = null)
    {
        this.printingConfig = printingConfig;
        MemberSelector = memberSelector;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    { 
        printingConfig.AddTypeSerializer(print);
        return printingConfig;
    }
    
    public PrintingConfig<TOwner> Using(CultureInfo culture)
    {
        if (!typeof(TPropType).IsValueType || !typeof(IFormattable).IsAssignableFrom(typeof(TPropType)))
        {
            throw new InvalidOperationException(
                $"Culture can only be applied to numeric or formattable types. Type: {typeof(TPropType)}");
        }

        printingConfig.AddTypeCulture<TPropType>(culture);
        return printingConfig;
    }
    
    PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
}