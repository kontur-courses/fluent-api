using System;
using System.Linq.Expressions;

namespace Homework.IgnoreContexts
{
    public interface IIgnoreConfigurator<TOwner>
    {
        public IIgnoreTypeConfigurator<TOwner> Type<T>();
        public IIgnoreTypeIntermediateConfigurator<TOwner> Property<T>(Expression<Func<TOwner, T>> extractor);
    }
}