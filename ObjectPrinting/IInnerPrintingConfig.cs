using System;

namespace ObjectPrinting
{
    public interface IInnerPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer);
    }
}