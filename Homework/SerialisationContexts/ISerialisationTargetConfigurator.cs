using System;
using System.Linq.Expressions;

namespace Homework.SerialisationContexts
{
    public interface ISerialisationTargetConfigurator<TOwner>
    {
        public SerialisationConfigurator<TOwner, T> For<T>(Expression<Func<TOwner, T>> extractor);
        public SerialisationConfigurator<TOwner, T> For<T>();
    }
}