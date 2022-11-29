using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public class ObjectConfigurator<TOwner> : IBasicConfigurator<TOwner>
{

    public IBasicConfigurator<TOwner> Exclude<T>()
    {
        excludedTypes.Add(typeof(T));
        return this;
    }

    public IBasicConfigurator<TOwner> Exclude<T>(Expression<Func<TOwner, T>> expression)
    {
        var memberExpression = (MemberExpression) expression.Body;
        var member = memberExpression.Member;
        excludedMembers.Add(member);
        return this;
    }

    public IMemberConfigurator<TOwner, T> ConfigureProperty<T>(Expression<Func<TOwner, T>> expression)
    {
        var memberExpression = (MemberExpression) expression.Body;
        var member = memberExpression.Member;
        excludedMembers.Add(member);
        return new MemberConfigurator<TOwner, T>(this);
        // return this;
    }

    public PrintingConfig<TOwner> ConfigurePrinter()
    {
        return new PrintingConfig<TOwner>();
    }

    private readonly HashSet<Type> excludedTypes = new();
    private readonly HashSet<MemberInfo> excludedMembers = new();
}