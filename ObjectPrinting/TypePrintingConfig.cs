using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting;

public class TypePrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> printingConfig;
    private readonly Dictionary<Type, Func<object, string>> serializers;

    public TypePrintingConfig(PrintingConfig<TOwner> printingConfig,
        Dictionary<Type, Func<object, string>> serializers)
    {
        if (printingConfig == null || serializers == null)
            throw new ArgumentNullException();

        this.printingConfig = printingConfig;
        this.serializers = serializers;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
    {
        if (serializer == null)
            throw new ArgumentNullException();

        serializers[typeof(TPropType)] = x => serializer((TPropType) x);
        return printingConfig;
    }
}