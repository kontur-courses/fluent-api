using System;

namespace ObjectPrinting
{
    public interface IInnerPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> Using(Func<TPropType, string> serializer);
    }
}