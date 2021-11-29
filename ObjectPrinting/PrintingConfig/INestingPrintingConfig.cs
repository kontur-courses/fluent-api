using System;

namespace ObjectPrinting.PrintingConfig
{
    public interface INestingPrintingConfig<TOwner, out TType>
    {
        PrintingConfig<TOwner> Use(Func<TType, string> transformer);
    }
}