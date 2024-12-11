using System.Globalization;
using ObjectSerializer.Interfaces;

namespace ObjectSerializer.ConcreteConfigs;

public class TypePrintConfig<TOwner, TType> : ITypePrintConfig<TOwner, TType>
{
    private readonly PrintingConfig<TOwner> printingConfig;

    public TypePrintConfig(PrintingConfig<TOwner> printingConfig)
    {
        this.printingConfig = printingConfig;
    }

    public IPrintingConfig<TOwner> Using(CultureInfo cultureInfo)
    {
        printingConfig.TypeCultureInfos.Add(typeof(TType), cultureInfo);

        return printingConfig;
    }

    public IPrintingConfig<TOwner> Using(Func<TType, string> printTypeFunc)
    {
        var funcStorage = printingConfig.TypePrintFunc;

        funcStorage[typeof(TType)] = p => printTypeFunc((TType)p);

        return printingConfig;
    }
}