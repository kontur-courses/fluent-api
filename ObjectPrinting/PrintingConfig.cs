using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using ObjectPrinting.Configurations;
using ObjectPrinting.Helpers;
using ObjectPrinting.Models;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>()
{
    private readonly HashSet<Type> excludeTypes = [];
    private readonly HashSet<PropertyInfo> excludeProperties = [];
    private readonly Dictionary<PropertyInfo, Delegate> propertyPrintingConfigs = new();
    private readonly Dictionary<Type, Delegate> typePrintingConfigs = new();
    private readonly Dictionary<Type, CultureInfo> typeCultureConfigs = new();
    private CultureInfo commonCulture = CultureInfo.CurrentCulture;

    public PrintingConfig<TOwner> UsingCommonCulture(CultureInfo culture)
    {
        commonCulture = culture;
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        excludeTypes.Add(typeof(TPropType));
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var propertyInfo = ReflectionHelper.GetPropertyInfoFromExpression(memberSelector);
        excludeProperties.Add(propertyInfo);
        return this;
    }

    public TypePrintingConfig<TOwner, TType> Printing<TType>()
    {
        return new TypePrintingConfig<TOwner, TType>(this);
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var propertyInfo = ReflectionHelper.GetPropertyInfoFromExpression(memberSelector);
        return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
    }

    internal PrintingConfig<TOwner> SetTypeCulture<TType>(CultureInfo culture)
        where TType : IFormattable
    {
        typeCultureConfigs[typeof(TType)] = culture;
        return this;
    }

    internal PrintingConfig<TOwner> AddTypeSerialize<TType>(Func<TType, string> print)
    {
        var type = typeof(TType);
        typePrintingConfigs[type] = print;
        return this;
    }

    internal PrintingConfig<TOwner> AddPropSerialize<TPropType>(PropertyInfo propertyInfo,
        Func<TPropType, string> print)
    {
        propertyPrintingConfigs[propertyInfo] = print;
        return this;
    }

    public string PrintToString(TOwner obj)
    {
        var serializeSettings = new SerializeSettings(commonCulture, excludeTypes, excludeProperties,
            propertyPrintingConfigs, typePrintingConfigs, typeCultureConfigs);
        var serializer = new Serializer(serializeSettings);
        return serializer.PrintToString(obj);
    }
}