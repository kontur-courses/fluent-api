using System;
using System.Linq.Expressions;

namespace ObjectPrinting;

public interface IBasicConfigurator<TOwner>
{
    IBasicConfigurator<TOwner> Exclude<T>();
    IBasicConfigurator<TOwner> Exclude<T>(Expression<Func<TOwner, T>> expression);
    IMemberConfigurator<TOwner, T> ConfigureProperty<T>(Expression<Func<TOwner, T>> expression);
    PrintingConfig<TOwner> ConfigurePrinter();
}