using System;

namespace ObjectPrinting
{
    public interface IPropertyConfig<TOwner, T>
    {
        IPropertyConfig<TOwner, T> OverrideSerializeMethod(Func<T, string> method);
        PrintingConfig<TOwner> SetConfig();
        PrintingConfig<TOwner> ExcludeFromConfig();
    }
}