using System;
using System.Globalization;
using ObjectPrinting.Configurations.Interfaces;

namespace ObjectPrinting.Configurations;

public class TypePrintingConfig<TOwner, TType>(PrintingConfig<TOwner> printingConfig) 
    : ITypePrintingConfig<TOwner>
{
    public CultureInfo? CultureInfo { get; private set; }
    public PrintingConfig<TOwner> ParentConfig { get; } = printingConfig;
    public Func<object, string>? Serializer { get; private set; }

    public PrintingConfig<TOwner> Using(Func<TType, string> print)
    {
        ArgumentNullException.ThrowIfNull(print);
        Serializer = printObject =>
        {
            if (printObject is not TType typeObject)
            {
                throw new ArgumentException($"The type {printObject} is not of type {typeof(TType)}");
            }

            return print(typeObject);
        };
        return ParentConfig;
    }

    internal PrintingConfig<TOwner> Using(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        CultureInfo = culture;
        return ParentConfig;
    }
}