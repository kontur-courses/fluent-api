using System;

namespace ObjectPrinting
{
    public interface INestingPrintingConfig<TOwner, out TType>
    {
        PrintingConfig<TOwner> Use(Func<TType, string> transformer);
    }
}