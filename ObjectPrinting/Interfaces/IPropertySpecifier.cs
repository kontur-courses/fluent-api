using System;

namespace ObjectPrinting.Interfaces
{
    public interface IPropertySpecifier<out T, TOwner>
    {
        public PrintingConfig<TOwner> With(Func<T, string> serializer);
    }
}