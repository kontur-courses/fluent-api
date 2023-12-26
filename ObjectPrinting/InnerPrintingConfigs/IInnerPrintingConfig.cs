using System;

namespace ObjectPrinting.InnerPrintingConfigs
{
    public interface IInnerPrintingConfig<TOwner, TType>
    {
        PrintingConfig<TOwner> Using(Func<TType, string> print);
        PrintingConfig<TOwner> TrimmedToLength(int maxLen);
    }
}