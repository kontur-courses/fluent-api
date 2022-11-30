using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using ObjectPrinting.MemberConfigurator;
using ObjectPrinting.MemberConfigurator.Implementation;

namespace ObjectPrinting.ObjectConfiguration.Implementation;

public class ObjectConfiguration<TOwner> : IObjectConfiguration<TOwner>
{
    public Dictionary<MemberInfo, Func<object, string>> MemberInfoConfigs { get; }
    public Dictionary<Type, Func<object, string>> TypeConfigs { get; }
    public HashSet<Type> ExcludedTypes { get; }
    public HashSet<MemberInfo> ExcludedMembers { get; }

    public ObjectConfiguration()
    {
        MemberInfoConfigs = new Dictionary<MemberInfo, Func<object, string>>();
        TypeConfigs = new Dictionary<Type, Func<object, string>>();
        ExcludedTypes = new HashSet<Type>();
        ExcludedMembers = new HashSet<MemberInfo>();
    }
    
    public IMemberConfigurator<TOwner, T> ConfigureType<T>() => new TypeConfigurator<TOwner, T>(this);
    public IObjectConfiguration<TOwner> ConfigureType<T>(Func<T, string> func) => ConfigureType<T>().Configure(func);

    public IObjectConfiguration<TOwner> Exclude<T>()
    {
        ExcludedTypes.Add(typeof(T));
        return this;
    }

    public IObjectConfiguration<TOwner> Exclude<T>(Expression<Func<TOwner, T>> expression)
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

    public PrintingConfig<TOwner> Build() => new(this);
}