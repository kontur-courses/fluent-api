using System.Reflection;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.PropertyPrintingConfig;

public class PropertyPrintingConfig<TOwner, TPropType>(
    PrintingConfig<TOwner> printingConfig)
    : IPropertyPrintingConfig<TOwner, TPropType>
{
    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        printingConfig.TypeSerializationMethod.AddMethod(typeof(TPropType), print);
        return printingConfig;
    }

    PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
}