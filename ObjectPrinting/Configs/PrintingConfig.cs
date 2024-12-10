using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ObjectPrinting.Configs.Children;
using ObjectPrinting.Tools;

namespace ObjectPrinting.Configs;

public class PrintingConfig<TOwner>
{
    private const int MAX_RECURSION = 4;
    
    private int? maxStringLen = null;
    
    private readonly HashSet<Type> excludedTypes = [];
    private readonly HashSet<string> excludedProperties = [];

    private IFormatProvider numberCulture = CultureInfo.InvariantCulture;
    
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
        return PrintToString(obj, 0);
    }

    private string PrintToString(object obj, int nestingLevel)
    {
        //TODO apply configurations
        if (obj == null)
            return "null" + Environment.NewLine;

        var finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        if (finalTypes.Contains(obj.GetType()))
            return obj + Environment.NewLine;

        var identation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        var type = obj.GetType();
        sb.AppendLine(type.Name);
        foreach (var propertyInfo in type.GetProperties())
        {
            sb.Append(identation + propertyInfo.Name + " = " +
                      PrintToString(propertyInfo.GetValue(obj),
                          nestingLevel + 1));
        }
        return sb.ToString();
    }
}