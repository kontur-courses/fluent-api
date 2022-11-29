using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public interface IPropertyConfig<TOwner, T>
    {
        PrintingConfig<TOwner> Printer { get; }
        Expression<Func<TOwner, T>> PropertyExpression { get; }
        IPropertyConfig<TOwner, T> OverrideSerializeMethod(Func<T, string> method);
        PrintingConfig<TOwner> SetConfig();
        PrintingConfig<TOwner> ExcludeFromConfig();
    }
}