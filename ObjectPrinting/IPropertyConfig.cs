using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public interface IPropertyConfig<TOwner, T>
    {
        PrintingConfig<TOwner> Printer { get; }
        Expression<Func<TOwner, T>> PropertyExpression { get; }
        PrintingConfig<TOwner> UseSerializeMethod(Func<T, string> method);
    }
}