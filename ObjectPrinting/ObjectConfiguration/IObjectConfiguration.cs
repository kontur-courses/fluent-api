using System;
using System.Linq.Expressions;
using ObjectPrinting.MemberConfigurator;

namespace ObjectPrinting.ObjectConfiguration;

public interface IObjectConfiguration<TOwner>
{
    IObjectConfiguration<TOwner> Exclude<T>();
    IObjectConfiguration<TOwner> Exclude<T>(Expression<Func<TOwner, T>> expression);
    IMemberConfigurator<TOwner, T> ConfigureProperty<T>(Expression<Func<TOwner, T>> expression);
    IMemberConfigurator<TOwner, T> ConfigureType<T>();
    PrintingConfig<TOwner> Build();
}