using System;
using ObjectPrinting.Configurations.Interfaces;

namespace ObjectPrinting.Configurations;

public class PropertyPrintingConfig<TOwner, TPropType>(PrintingConfig<TOwner> printingConfig)
    : IPropertyPrintingConfig<TOwner>
{
    public PrintingConfig<TOwner> ParentConfig { get; } = printingConfig;
    public Func<object, string> Serializer { get; private set; }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
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