using System;
using System.Globalization;

namespace ObjectPrinting.Serializer.Configs.Children;

public class TypeConfig<TOwner, TPropType>(PrintingConfig<TOwner> printingConfig) : IChildConfig<TOwner, TPropType>
{
    public PrintingConfig<TOwner> ParentConfig { get; } = printingConfig;
    
    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        ParentConfig.TypeSerializers[typeof(TPropType)] = print;
        return ParentConfig;
    }
}