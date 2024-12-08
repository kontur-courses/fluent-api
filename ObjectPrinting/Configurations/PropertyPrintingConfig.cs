using System;
using ObjectPrinting.Configurations.Interfaces;

namespace ObjectPrinting.Configurations;

public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
{
    public PrintingConfig<TOwner> ParentConfig { get; }
    public Func<object, string>? Serializer { get; private set; }

    public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
    {
        ParentConfig = printingConfig;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        ArgumentNullException.ThrowIfNull(print);
        Serializer = printObject =>
        {
            if (printObject is not TPropType propertyObject)
            {
                throw new ArgumentException($"The property {printObject} is not of type {typeof(TPropType)}");
            }

            return print(propertyObject);
        };
        return ParentConfig;
    }
}