using System;

namespace ObjectPrinting
{
    public interface IInnerTypeConfig<TOwner, TType>
    {
        PrintingConfig<TOwner> Using(Func<TType, string> transformer);
    }
}