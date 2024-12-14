using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>(PrintingConfig<TOwner> printingConfig, 
    Expression<Func<TOwner, TPropType>> memberSelector = null!) : IPropertyPrintingConfig<TOwner, TPropType>
{
    public Expression<Func<TOwner, TPropType>> MemberSelector => memberSelector;
    
    public PrintingConfig<TOwner> Using(Func<TPropType, string> print) => printingConfig.AddSerializerForType(print);

    public PrintingConfig<TOwner> Using(CultureInfo culture)
    {
        if (!typeof(TPropType).IsValueType || !typeof(IFormattable).IsAssignableFrom(typeof(TPropType)))
            throw new InvalidOperationException($"Can't apply culture for type: {typeof(TPropType)}");
        
        return printingConfig.AddCultureForType<TPropType>(culture);
    }

    PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
}

public interface IPropertyPrintingConfig<TOwner, TPropType>
{
    PrintingConfig<TOwner> ParentConfig { get; }
}