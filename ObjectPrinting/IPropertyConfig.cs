using System;

namespace ObjectPrinting
{
    public interface IPropertyConfig<TOwner,T>
    {
        IPropertyConfig<TOwner, T> AlternateSerializeMethod(Func<T, string> method);
        PrintingConfig<T> SetConfig();
    }
}