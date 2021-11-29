using System;

namespace ObjectPrinting
{
    public interface IMemberPrintingConfig<TOwner, T>
    {
        PrintingConfig<TOwner> Using(Func<T, string> serializationRule);
    }
}
