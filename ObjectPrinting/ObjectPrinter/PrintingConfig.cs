using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting.ObjectPrinter;

public class PrintingConfig<TOwner>
{
    public readonly HashSet<Type> ExcludedTypes = [];
    public readonly HashSet<MemberInfo> ExcludedMember = [];
    public readonly Dictionary<MemberInfo, int> TrimmedMembers = new();
    public readonly Dictionary<Type, CultureInfo> CulturesForTypes = new();
    internal readonly Dictionary<Type, Func<object, string>> CustomTypeSerializers = new();
    internal readonly Dictionary<MemberInfo, Func<object, string>> CustomMemberSerializers = new();
    public readonly Dictionary<Type, int> TrimmedTypes = new();

    public int MaxNestingLevel { get; init; } = 5;

    public PrintingConfig<TOwner> Excluding<TMember>()
    {
        ExcludedTypes.Add(typeof(TMember));
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TMember>(Expression<Func<TOwner, TMember>> memberSelector)
    {
        ExcludedMember.Add(GetMember(memberSelector));
        return this;
    }

    public MemberPrintingConfig<TOwner, TMember> Printing<TMember>()
        => new(this);

    public StringPrintConfig<TOwner> Printing(Expression<Func<TOwner, string>> stringMemberSelector)
        => new(this, GetMember(stringMemberSelector));
    
    public MemberPrintingConfig<TOwner, TMember> Printing<TMember>(Expression<Func<TOwner, TMember>> memberSelector)
        => new(this, GetMember(memberSelector));


    public string PrintToString(TOwner? obj)
        => Serializer.SerializeObject(obj, this);


    private static MemberInfo GetMember<TMember>(Expression<Func<TOwner, TMember>> selector)
    {
        if (selector.Body is MemberExpression m)
            return m.Member;
        throw new ArgumentException("Member selector must be a simple member access.", nameof(selector));
    }
}