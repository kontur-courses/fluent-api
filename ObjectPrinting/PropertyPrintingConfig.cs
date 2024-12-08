using System;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>(
    PrintingConfig<TOwner> printingConfig, PropertyInfo? propertyInfo = null)
    : IPropertyPrintingConfig<TOwner, TPropType>
{
    internal readonly PrintingConfig<TOwner> PrintingConfig = printingConfig; 
    internal readonly PropertyInfo? PropertyInfo = propertyInfo;
    
    public PrintingConfig<TOwner> Using(Func<TPropType, string> printingMethod)
    {
        if (PropertyInfo is null)
            PrintingConfig.CustomTypeSerializers[typeof(TPropType)] = printingMethod;
        else
            PrintingConfig.CustomPropertySerializers[PropertyInfo] = printingMethod;

        return PrintingConfig;
    }

    PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => PrintingConfig;
}

public interface IPropertyPrintingConfig<TOwner, TPropType>
{
    PrintingConfig<TOwner> ParentConfig { get; }
}