using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    internal readonly HashSet<Type> ExcludedTypes = new();
    internal readonly HashSet<MemberInfo> ExcludedMembers = new();

    internal Dictionary<Type, Delegate> CustomTypeSerializers { get; } = new();
    internal Dictionary<MemberInfo, Delegate> CustomMemberSerializers { get; } = new();
    internal Dictionary<Type, CultureInfo> CulturesForTypes { get; } = new();
    internal Dictionary<MemberInfo, int> TrimmedMembers { get; } = new();

    public MemberPrintingConfig<TOwner, TMemberType> SetPrintingFor<TMemberType>()
    {
        return new MemberPrintingConfig<TOwner, TMemberType>(this);
    }

    public MemberPrintingConfig<TOwner, TMemberType> SetPrintingFor<TMemberType>(
        Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        var expression = (MemberExpression)memberSelector.Body;
        return new MemberPrintingConfig<TOwner, TMemberType>(this, expression.Member);
    }

    public PrintingConfig<TOwner> ExcludeMember<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        var expression = (MemberExpression)memberSelector.Body;
        ExcludedMembers.Add(expression.Member);

        return this;
    }

    public PrintingConfig<TOwner> ExcludeMemberType<TMemberType>()
    {
        ExcludedTypes.Add(typeof(TMemberType));

        return this;
    }

    public string PrintToString(TOwner obj)
    {
        var serializer = new Serializer<TOwner>(this);

        return serializer.SerializeObject(obj);
    }
}