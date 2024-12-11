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
    
    private readonly HashSet<Type> excludedTypes = [];
    private readonly HashSet<string> excludedProperties = [];

    private readonly Dictionary<Type, CultureInfo> culturesForTypes = new();
    private readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
    private readonly Dictionary<string, Func<object, string>> propertySerializers = new();

    public TypeConfig<TOwner, TPropType> Printing<TPropType>() 
        => new(this);

    public PropertyConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector) 
        => new(this, memberSelector);
    
    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        excludedTypes.Add(typeof(TPropType));
        return this;
    }
    
    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        excludedProperties.Add(memberSelector.TryGetPropertyName());
        return this;
    }

    public string PrintToString(TOwner obj)
    {
        var serializer = new ConfigSerializer();
        return serializer.PrintToString(obj, 0);
    }
}