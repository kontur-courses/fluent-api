using System;

namespace ObjectPrinting
{
    public interface IMemberPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> Using(Func<TPropType, string> format);
    }
}