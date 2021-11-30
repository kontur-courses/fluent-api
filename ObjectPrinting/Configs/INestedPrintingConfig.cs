using System;

namespace ObjectPrinting.Configs
{
    public interface INestedPrintingConfig<TOwner, out TType>
    {
        PrintingConfig<TOwner> With(Func<TType, string> serializer);
    }
}