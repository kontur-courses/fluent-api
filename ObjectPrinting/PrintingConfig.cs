using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ObjectPrinting.Configs;
using ObjectPrinting.Extensions;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    public Dictionary<Type, Func<object, string?>> TypeSerializers { get; } = new();
    public Dictionary<string, Func<object, string>> PropertySerializers { get; } = new();
    public HashSet<Type> ExcludedTypes { get; } = [];
    public HashSet<string> ExcludedProperties { get; } = [];
    
    public readonly HashSet<object?> Visited = [];
    public Dictionary<string, int> StringTrimmingRules { get; } = new();

    public PrintingConfig<TOwner> Excluding<TProp>()
    {
        ExcludedTypes.Add(typeof(TProp));
        return this;
    }
    public PrintingConfig<TOwner> Excluding(Expression<Func<TOwner, object>> selector)
    {
        ExcludedProperties.Add(PrintingConfigExtensions.GetPropertyName(selector));
        return this;
    }

    public TypePrintingConfig<TOwner, TProp> Printing<TProp>()
    {
        return new TypePrintingConfig<TOwner, TProp>(this);
    }

    public PropertyPrintingConfig<TOwner, TProp> Printing<TProp>(
        Expression<Func<TOwner, TProp>> selector)
    {
        return new PropertyPrintingConfig<TOwner, TProp>(this, selector);
    }
}