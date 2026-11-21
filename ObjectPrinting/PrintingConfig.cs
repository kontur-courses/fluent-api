using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ObjectPrinting;

public sealed class PrintingConfig<TOwner>
{
    private readonly PrintingRules _rules;

    internal PrintingRules Rules => _rules;

    internal PrintingConfig(PrintingRules? rules = null)
    {
        _rules = rules ?? new PrintingRules();
    }

    //1. Исключить из сериализации свойства определенного типа
    public PrintingConfig<TOwner> Exclude<TMember>() => new(_rules.ExcludeType(typeof(TMember)));

    //6. Исключить из сериализации конкретное свойство
    public PrintingConfig<TOwner> Exclude<TMember>(Expression<Func<TOwner, TMember>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));

        var member = ExtractMemberPath(selector);

        return new(_rules.ExcludeMember(member));
    }

    // переход в доп.контекст типа
    public TypeConfig<TOwner, TType> For<TType>()
    {
        return new(this);
    }

    // переход в доп.контекст свойства
    public MemberConfig<TOwner, TMember> For<TMember>(Expression<Func<TOwner, TMember>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));

        var member = ExtractMemberPath(selector);

        return new(this, member);
    }

    // выход из конфигурации
    public ObjectPrinter<TOwner> Apply() => new(_rules);

    private static MemberPath ExtractMemberPath<TMember>(Expression<Func<TOwner, TMember>> memberSelector)
    {
        ArgumentNullException.ThrowIfNull(memberSelector, nameof(memberSelector));

        var parts = new Stack<string>();

        Expression? current = memberSelector.Body;

        if (current is UnaryExpression { NodeType: ExpressionType.Convert } unary)
            current = unary.Operand;

        while (current is MemberExpression memberExpr)
        {
            parts.Push(memberExpr.Member.Name);

            current = memberExpr.Expression;
        }

        if (current != memberSelector.Parameters[0] || parts.Count == 0)
            throw new ArgumentException("Expression must be a member access path, e.g. 'p => p.Prop' or 'p => p.Nested.Prop'", nameof(memberSelector));

        return new(string.Join(".", parts));
    }
}