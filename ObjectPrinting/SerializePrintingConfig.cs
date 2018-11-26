using System;

namespace ObjectPrinting
{
    public class SerializePrintingConfig<TOwner, T>
    {
        public PrintingConfig<TOwner> As(Func<T, T> serializer)
        {
            return new PrintingConfig<TOwner>();
        }
    }
}