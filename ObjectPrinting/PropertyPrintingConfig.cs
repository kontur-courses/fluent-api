using System;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>(PrintingConfig<TOwner> printingConfig, string propertyName)
    : IPropertyPrintingConfig<TOwner, TPropType>
{
    public string PropertyName => propertyName;
    
    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        if (!printingConfig.PropertyPrinters.TryAdd(propertyName, obj => print((TPropType)obj)))
            printingConfig.PropertyPrinters[propertyName] = obj => print((TPropType)obj);
        return printingConfig;
    }

    PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
}

public interface IPropertyPrintingConfig<TOwner, out TPropType>
{
    string PropertyName { get; }
    PrintingConfig<TOwner> ParentConfig { get; }
    PrintingConfig<TOwner> Using(Func<TPropType, string> print);
}