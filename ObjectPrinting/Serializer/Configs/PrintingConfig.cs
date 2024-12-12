using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using ObjectPrinting.Serializer.Configs.Children;
using ObjectPrinting.Tools;

namespace ObjectPrinting.Serializer.Configs;

public class PrintingConfig<TOwner>
{
    public uint MaxNestingLevel { get; private set; } = 10;

    internal readonly HashSet<Type> ExcludedTypes = [];
    internal readonly HashSet<string> ExcludedProperties = [];
    
    internal uint? TrimStringValue;
    internal readonly Dictionary<string, uint> TrimmedMembers = new();

    internal readonly Dictionary<Type, CultureInfo> CulturesForTypes = new();
    internal readonly Dictionary<Type, Func<object, string>> TypeSerializers = new();
    internal readonly Dictionary<string, Func<object, string>> PropertySerializers = new();

    public TypeConfig<TOwner, TPropType> Printing<TPropType>() 
        => new(this);

    public PropertyConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector) 
        => new(this, memberSelector);

    public PrintingConfig<TOwner> WithMaxNestingLevel(uint level)
    {
        MaxNestingLevel = level;
        return this;
    }
    
    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        ExcludedTypes.Add(typeof(TPropType));
        return this;
    }
    
    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        ExcludedProperties.Add(memberSelector.TryGetPropertyName());
        return this;
    }

    public string PrintToString(TOwner obj)
    {
        var serializer = new ConfigSerializer<TOwner>(this);
        return serializer.PrintToString(obj, 0);
    }
}
