using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public interface IBaseConfig<TOwner>
    {
        string PrintToString(TOwner obj);
        IBaseConfig<TOwner> Exclude<T>();
        IBaseConfig<TOwner> Exclude<TArg>(Expression<Func<TOwner, TArg>> f);
        TypeConfig<TOwner, TArg> Printing<TArg>();
        PropertyConfig<TOwner, TProperty> Printing<TProperty>(Expression<Func<TOwner, TProperty>> f);
    }
}