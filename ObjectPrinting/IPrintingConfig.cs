using System;

namespace ObjectPrinting
{
    public interface IPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> With<TPropType>(Func<TPropType, string> printer);
    }
}
