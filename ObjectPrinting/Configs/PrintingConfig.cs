using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting.Configs;

public class PrintingConfig<TOwner>
{
    private readonly PrintingContext context;
    private readonly Printer printer;

    public PrintingConfig()
    {
        context = new PrintingContext();
        printer = new Printer(context);
    }

    public string PrintToString(TOwner? obj)
    {
        return printer.PrintToString(obj, 0);
    }

    public TypePrintingConfig<TOwner, TProp> For<TProp>()
    {
        return new TypePrintingConfig<TOwner, TProp>(this, context);
    }

    public MemberPrintingConfig<TOwner, TProp> For<TProp>(Expression<Func<TOwner, TProp>> memberSelector)
    {
        var member = GetMemberInfo(memberSelector);
        return new MemberPrintingConfig<TOwner, TProp>(this, member, context);
    }

    public StringMemberPrintingConfig<TOwner> For(Expression<Func<TOwner, string>> memberSelector)
    {
        var member = GetMemberInfo(memberSelector);
        return new StringMemberPrintingConfig<TOwner>(this, member, context);
    }

    public PrintingConfig<TOwner> Exclude<TProp>()
    {
        context.excludedTypes.Add(typeof(TProp));
        return this;
    }

    public PrintingConfig<TOwner> Exclude<TProp>(Expression<Func<TOwner, TProp>> memberSelector)
    {
        context.excludedMembers.Add(GetMemberInfo(memberSelector));
        return this;
    }

    private MemberInfo GetMemberInfo<TProp>(Expression<Func<TOwner, TProp>> memberSelector)
    {
        var body = memberSelector.Body;
        if (body is MemberExpression memberExpr)
            return memberExpr.Member;

        throw new ArgumentException("Expression is not a member access", nameof(memberSelector));
    }
}