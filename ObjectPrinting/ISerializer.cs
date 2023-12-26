using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public interface ISerializer<TOwner>
    {
        string PrintToString(TOwner obj);
        PropertySetting<TOwner> SelectProperty<P>(Expression<Func<TOwner, P>> properties);
        ISerializer<TOwner> ChangeProperty<T>(Func<object, string> method);
        ISerializer<TOwner> Exclude<T>();
    }
}