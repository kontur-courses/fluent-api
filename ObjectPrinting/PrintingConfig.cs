using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public class PrintingConfig<TOwner> : IPrintingSettings
{
    private readonly HashSet<Type> _excludedTypes = [];
    private readonly HashSet<MemberInfo> _excludedMembers = [];
    private readonly Dictionary<Type, Func<object, string>> _typePrinters = [];
    private readonly Dictionary<MemberInfo, Func<object, string>> _memberPrinters = [];

    public string PrintToString(TOwner obj)
    {
        return ObjectSerializer.Serialize(obj, this);
    }

    public PrintingConfig<TOwner> Exclude<TExcludingType>()
    {
        ExcludeType(typeof(TExcludingType));
        return this;
    }

    public PrintingConfig<TOwner> Exclude<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        ExcludeMember(GetSelectedMember(memberSelector));
        return this;
    }

    public ValuePrintingConfig<TOwner, TPrintingType> Printing<TPrintingType>()
    {
        return new ValuePrintingConfig<TOwner, TPrintingType>(
            this,
            printer => _typePrinters[typeof(TPrintingType)] = printer);
    }

    public ValuePrintingConfig<TOwner, TMemberType> Printing<TMemberType>(
        Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        var member = GetSelectedMember(memberSelector);
        return new ValuePrintingConfig<TOwner, TMemberType>(
            this,
            printer => _memberPrinters[member] = printer);
    }

    bool IPrintingSettings.IsExcluded(MemberInfo member, Type memberType)
    {
        return _excludedMembers.Contains(member)
            || _excludedTypes.Contains(memberType);
    }

    bool IPrintingSettings.TryGetTypePrinter(Type type, out Func<object, string> printer)
    {
        return _typePrinters.TryGetValue(type, out printer);
    }

    bool IPrintingSettings.TryGetMemberPrinter(MemberInfo member, out Func<object, string> printer)
    {
        return _memberPrinters.TryGetValue(member, out printer);
    }

    internal void ExcludeType(Type excludedType)
    {
        _excludedTypes.Add(excludedType);
    }

    internal void ExcludeMember(MemberInfo excludedMember)
    {
        _excludedMembers.Add(excludedMember);
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