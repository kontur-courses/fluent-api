using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly Dictionary<Type, PrintingRule> _typeRules = new();
    private readonly Dictionary<MemberInfo, PrintingRule> _memberRules = new();

    public PrintingConfig<TOwner> Excluding<TType>()
    {
        var rule = GetOrCreateRule<TType>();
        rule.IsExcluded = true;

        return this;
    }

    public PrintingConfig<TOwner> Excluding<TType>(Expression<Func<TOwner, TType>> selector)
    {
        var member = GetDirectMember(selector);
        var rule = GetOrCreateRule<TType>(member);

        rule.IsExcluded = true;
        return this;
    }

    public PropertyPrintingConfig<TOwner, TType> Printing<TType>()
    {
        return new PropertyPrintingConfig<TOwner, TType>(this);
    }

    public PropertyPrintingConfig<TOwner, TType> Printing<TType>(
        Expression<Func<TOwner, TType>> selector)
    {
        return new PropertyPrintingConfig<TOwner, TType>(
            this, GetDirectMember(selector));
    }

    public string PrintToString(TOwner obj)
    {
        var serializer = new ObjectSerializer(_typeRules, _memberRules);
        return serializer.Serialize(obj, typeof(TOwner));
    }

    internal PrintingRule GetOrCreateRule<TType>(MemberInfo? member = null)
    {
        return member is null
            ? GetOrCreateRule(_typeRules, typeof(TType))
            : GetOrCreateRule(_memberRules, PrintableMember.GetKey(member));
    }

    private static PrintingRule GetOrCreateRule<TKey>(
        Dictionary<TKey, PrintingRule> rules, TKey key)
        where TKey : notnull
    {
        if (!rules.TryGetValue(key, out var rule))
        {
            rule = new PrintingRule();
            rules.Add(key, rule);
        }

        return rule;
    }

    private static MemberInfo GetDirectMember<TType>(Expression<Func<TOwner, TType>> selector)
    {
        if (selector.Body is not MemberExpression expression ||
            expression.Expression != selector.Parameters[0])
        {
            throw new ArgumentException(
                "Свойство должно быть указано напрямую, например p => p.Age");
        }

        return expression.Member;
    }
}
