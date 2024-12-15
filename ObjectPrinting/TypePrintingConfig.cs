using System;

namespace ObjectPrinting;

public class TypePrintingConfig<TOwner, TType>(PrintingConfig<TOwner> printingConfig) : ITypePrintingConfig<TOwner, TType>
{
    public PrintingConfig<TOwner> ParentConfig => printingConfig;
    
    public PrintingConfig<TOwner> Using(Func<TType, string> print)
    {
        ParentConfig.TypePrinters.TryAdd(typeof(TType), obj => print((TType)obj));
        return ParentConfig;
    }
}

public interface ITypePrintingConfig<TOwner, out TType>
{
    PrintingConfig<TOwner> ParentConfig { get; }
    PrintingConfig<TOwner> Using(Func<TType, string> print);
}