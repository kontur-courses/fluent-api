using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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

    public PropertyPrintingConfig<TOwner, TType> Printing<TType>(Expression<Func<TOwner, TType>> selector)
    {
        var member = GetDirectMember(selector);

        return new PropertyPrintingConfig<TOwner, TType>(this, member);
    }

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 0);
    }

    internal PrintingRule GetOrCreateRule<TType>(MemberInfo? member = null)
    {
        return member is null
            ? GetOrCreateRule(_typeRules, typeof(TType))
            : GetOrCreateRule(_memberRules, PrintableMember.GetKey(member));
    }

    private string PrintToString(object obj, int nestingLevel)
    {
        if (obj == null)
            return "null" + Environment.NewLine;

        var finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        if (finalTypes.Contains(obj.GetType()))
            return obj + Environment.NewLine;

        var identation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        var type = obj.GetType();
        sb.AppendLine(type.Name);
        foreach (var propertyInfo in type.GetProperties())
        {
            sb.Append(identation + propertyInfo.Name + " = " +
                      PrintToString(propertyInfo.GetValue(obj),
                          nestingLevel + 1));
        }
        return sb.ToString();
    }

    private static PrintingRule GetOrCreateRule<TKey>(
        Dictionary<TKey, PrintingRule> rules, TKey key)
        where TKey : notnull
    {
        if (!rules.TryGetValue(key, out var rule))
        {
            rule = new PrintingRule();
            rules[key] = rule;
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
