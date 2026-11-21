using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    internal PrintingSettings Settings { get; } = new ();

    public string PrintToString(TOwner obj)
    {
        return ObjectSerializer.Serialize(obj, Settings);
    }

    public PrintingConfig<TOwner> ExcludeType<TTypeToExclude>()
    {
        Settings.ExcludeType(typeof(TTypeToExclude));
        return this;
    }

    public PrintingConfig<TOwner> ExcludeMember<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        Settings.ExcludeMember(GetSelectedMember(memberSelector));
        return this;
    }

    public ValuePrintingConfig<TOwner, TPrintingType> PrintingType<TPrintingType>()
    {
        return new ValuePrintingConfig<TOwner, TPrintingType>(
            this,
            printer => Settings.SetTypePrinter(typeof(TPrintingType), printer));
    }

    public ValuePrintingConfig<TOwner, TMemberType> PrintingMember<TMemberType>(
        Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        var member = GetSelectedMember(memberSelector);
        return new ValuePrintingConfig<TOwner, TMemberType>(
            this,
            printer => Settings.SetMemberPrinter(member, printer));
    }

    private static MemberInfo GetSelectedMember<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        ArgumentNullException.ThrowIfNull(memberSelector);
        var memberExpression = memberSelector.Body as MemberExpression;
        ThrowIfNotOwnerMember(memberExpression, nameof(memberSelector));
        return memberExpression!.Member;
    }

    private static void ThrowIfNotOwnerMember(MemberExpression memberExpression, string paramName)
    {
        if (memberExpression?.Expression is ParameterExpression)
            return;

        throw new ArgumentException("Selector must return a member of the printed object", paramName);
    }
}