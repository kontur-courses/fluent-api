using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>
{
    private readonly PropertyInfo? propertyInfo;

    public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo? propertyInfo = null)
    {
        ParentConfig = printingConfig;
        this.propertyInfo = propertyInfo;
    }

    internal PrintingConfig<TOwner> ParentConfig { get; }


    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        if (propertyInfo is null)
            ParentConfig.AlternativePrintingsForTypes[typeof(TPropType)] = obj => print((TPropType)obj);
        else
            ParentConfig.AlternativePrintingsForProperties[propertyInfo] = obj => print((TPropType)obj);
        return ParentConfig;
    }
}