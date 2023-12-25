using System;
using System.Reflection;

namespace ObjectPrinting;

public class MemberPrintingConfig<TOwner, TMemberType>
{
    internal readonly PrintingConfig<TOwner> PrintingConfig;
    internal readonly PropertyInfo PropertyInfo;

    public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo = null)
    {
        PrintingConfig = printingConfig;
        PropertyInfo = propertyInfo;
    }

    public PrintingConfig<TOwner> Using(Func<TMemberType, string> printingMethod)
    {
        if (PropertyInfo is null)
            PrintingConfig.CustomTypeSerializers[typeof(TMemberType)] = printingMethod;
        else
            PrintingConfig.CustomPropertySerializers[PropertyInfo] = printingMethod;

        return PrintingConfig;
    }
}