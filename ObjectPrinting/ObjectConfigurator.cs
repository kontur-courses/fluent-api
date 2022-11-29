using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public class ObjectConfigurator<TOwner> : IBasicConfigurator<TOwner>
{
    public Dictionary<MemberInfo, UniversalConfig> MemberInfoConfigs { get; }
    public Dictionary<Type, UniversalConfig> TypeConfigs { get; }
    public HashSet<Type> ExcludedTypes { get; }
    public HashSet<MemberInfo> ExcludedMembers { get; }
    public ITypeConfigurator<TOwner, T> ConfigureType<T>()
    {
        return new TypeConfigurator<TOwner, T>(this);
    }

    public ObjectConfigurator()
    {
        MemberInfoConfigs = new Dictionary<MemberInfo, UniversalConfig>();
        TypeConfigs = new Dictionary<Type, UniversalConfig>();
        ExcludedTypes = new HashSet<Type>();
        ExcludedMembers = new HashSet<MemberInfo>();
    }

    public IBasicConfigurator<TOwner> Exclude<T>()
    {
        ExcludedTypes.Add(typeof(T));
        return this;
    }

    public IBasicConfigurator<TOwner> Exclude<T>(Expression<Func<TOwner, T>> expression)
    {
        var memberExpression = (MemberExpression) expression.Body;
        var member = memberExpression.Member;
        ExcludedMembers.Add(member);
        return this;
    }

    public IMemberConfigurator<TOwner, T> ConfigureProperty<T>(Expression<Func<TOwner, T>> expression)
    {
        var memberExpression = (MemberExpression) expression.Body;
        var member = memberExpression.Member;
        return new MemberConfigurator<TOwner, T>(this, member);
    }

    public PrintingConfig<TOwner> ConfigurePrinter()
    {
        return new PrintingConfig<TOwner>(this);
    }
}