using System;

namespace ObjectPrinting
{
    public interface IInnerPrintingConfig<TOwner, TPropType>
    {
        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer);
    }
}