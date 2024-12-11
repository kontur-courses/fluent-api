using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using ObjectPrinting.Serializer.Configs.Children;
using ObjectPrinting.Tools;

namespace ObjectPrinting.Serializer.Configs;

public class PrintingConfig<TOwner>
{
    private const int MAX_RECURSION = 4;
    
    private int? maxStringLen = null;
    
    internal HashSet<Type> ExcludedTypes = [];
    internal HashSet<string> ExcludedProperties = [];

    internal Dictionary<Type, Delegate> TypeSerializers = new();
    internal Dictionary<Type, CultureInfo> CulturesForTypes = new();
    internal Dictionary<string, Func<object, string>> PropertySerializers = new();

    public TypeConfig<TOwner, TPropType> Printing<TPropType>() 
        => new(this);

    public PropertyConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector) 
        => new(this, memberSelector);
    
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
        var serializer = new ConfigSerializer();
        return serializer.PrintToString(obj, 0);
    }
}