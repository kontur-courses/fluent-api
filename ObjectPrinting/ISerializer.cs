using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public interface ISerializer<T>
    {
        string PrintToString(T obj);
        ISerializer<T> ChangePropertyOutput<P>(Expression<Func<T, P>>  properties, Func<object, string> method);
        ISerializer<T> ExceptProperty<P>(Expression<Func<T, P>> properties);
        ISerializer<T> ExceptType(Type type);
    }
}