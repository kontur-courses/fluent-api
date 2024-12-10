namespace ObjectPrinting.PropertyPrintingConfig;

public class PropertyPrintingConfig<TOwner, TPropType>(PrintingConfig<TOwner> printingConfig)
    : IPropertyPrintingConfig<TOwner, TPropType>
{
    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        if (printingConfig.TypeSerializationMethod.TryAdd(typeof(TPropType), print))
            return printingConfig;

        throw new InvalidOperationException($"Type {typeof(TPropType).Name} is already registered");
    }

    public PrintingConfig<TOwner> Using(CultureInfo culture)
    {
        return printingConfig;
    }

    PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
}