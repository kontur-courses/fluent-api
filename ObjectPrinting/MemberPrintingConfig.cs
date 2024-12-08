using System;
using System.Reflection;

namespace ObjectPrinting;

public class MemberPrintingConfig<TOwner, TMemberType>(
    PrintingConfig<TOwner> printingConfig, PropertyInfo? propertyInfo = null)
    : IMemberPrintingConfig<TOwner, TMemberType>
{
    internal readonly PrintingConfig<TOwner> PrintingConfig = printingConfig; 
    internal readonly PropertyInfo? PropertyInfo = propertyInfo;
    
    public PrintingConfig<TOwner> Using(Func<TMemberType, string> printingMethod)
    {
        if (PropertyInfo is null)
            PrintingConfig.CustomTypeSerializers[typeof(TMemberType)] = printingMethod;
        else
            PrintingConfig.CustomPropertySerializers[PropertyInfo] = printingMethod;

        return PrintingConfig;
    }

    PrintingConfig<TOwner> IMemberPrintingConfig<TOwner, TMemberType>.ParentConfig => PrintingConfig;
}

public interface IMemberPrintingConfig<TOwner, TMemberType>
{
    PrintingConfig<TOwner> ParentConfig { get; }
}