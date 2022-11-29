using System;
using System.Linq.Expressions;

namespace ObjectPrinting;

public class ObjectConfigurator<TOwner> : IBasicConfigurator<TOwner>
{
    public IBasicConfigurator<TOwner> Exclude<T>()
    {
        throw new NotImplementedException();
    }

    public IBasicConfigurator<TOwner> Exclude<T>(Expression<Func<TOwner, T>> expression)
    {
        throw new NotImplementedException();
    }

    public IMemberConfigurator<TOwner, T> ConfigureProperty<T>(Expression<Func<TOwner, T>> expression)
    {
        throw new NotImplementedException();
    }

    public string PrintToString(TOwner obj)
    {
        throw new NotImplementedException();
    }
}