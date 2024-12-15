using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    internal readonly HashSet<Type> ExcludedTypes = [];
    internal readonly HashSet<MemberInfo?> ExcludedMembers = [];

    internal Dictionary<MemberInfo, int> TrimmedMembers { get; } = new();
    internal Dictionary<Type, CultureInfo> CulturesForTypes { get; } = new();
    internal Dictionary<Type, Func<object, string>> CustomTypeSerializers { get; } = new();
    internal Dictionary<MemberInfo, Func<object, string>> CustomMemberSerializers { get; } = new();
    
    internal int? TrimStringLength;
    private int maxNestingLevel = 5;
    
    public int MaxNestingLevel
    {
        get => maxNestingLevel; 
        private set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "The maxNestingDepth value must be positive.");
            }
            maxNestingLevel = value;
        }
    }

    public MemberPrintingConfig<TOwner, TMemberType> SetPrintingFor<TMemberType>() => new(this);
    
    public MemberPrintingConfig<TOwner, TMemberType> SetPrintingFor<TMemberType>(
        Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        var expression = (MemberExpression)memberSelector.Body;
        return new MemberPrintingConfig<TOwner, TMemberType>(this, expression.Member);
    }

    public PrintingConfig<TOwner> Exclude<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        var expression = (MemberExpression)memberSelector.Body;
        ExcludedMembers.Add(expression.Member);
        
        return this;
    }

    public PrintingConfig<TOwner> Exclude<TMemberType>()
    {
        ExcludedTypes.Add(typeof(TMemberType));
        return this;
    }

    public PrintingConfig<TOwner> SetSerializationDepth(int depth)
    {
        MaxNestingLevel = depth;
        return this;
    }

    public string PrintToString(TOwner? obj)
    {
        var serializer = new Serializer<TOwner>(this);

        return serializer.SerializeObject(obj);
    }
}