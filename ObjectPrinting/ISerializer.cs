using System;
using System.Globalization;

namespace ObjectPrinting
{
    public interface ISerializer<out T, TOwner>
    {
        public PrintingConfig<TOwner> PrintingConfig { get; }
        public PrintingConfig<TOwner> Serialize(Func<T, string> serializer);
    }
}