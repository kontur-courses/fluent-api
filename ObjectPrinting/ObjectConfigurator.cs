using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public class ObjectConfigurator<TOwner> : IBasicConfigurator<TOwner>
{
    public Dictionary<MemberInfo, MemberConfig> Dict { get; }
    public HashSet<Type> ExcludedTypes { get; }
    public HashSet<MemberInfo> ExcludedMembers { get; }

    public ObjectConfigurator()
    {
        Dict = new Dictionary<MemberInfo, MemberConfig>();
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

public class MemberConfig
{
    public CultureInfo CultureInfo { get; }
    public int TrimLength { get; }

    public MemberConfig(CultureInfo cultureInfo, int trimLength)
    {
        CultureInfo = cultureInfo;
        TrimLength = trimLength;
    }
}