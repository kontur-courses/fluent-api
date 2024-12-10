using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using ObjectPrinting.Configurations;
using ObjectPrinting.Configurations.Interfaces;
using ObjectPrinting.Helpers;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>(ObjectPrinter<TOwner> objectPrinter)
{
    private readonly HashSet<Type> excludeTypes = [];
    private readonly HashSet<PropertyInfo> excludeProperties = [];
    private readonly Dictionary<PropertyInfo, IPropertyPrintingConfig<TOwner>> propertyPrintingConfigs = new();
    private readonly Dictionary<Type, ITypePrintingConfig<TOwner>> typePrintingConfigs = new();
    internal CultureInfo CommonCulture { get; private set; } = CultureInfo.CurrentCulture;

    internal IReadOnlySet<Type> ExcludeTypes => excludeTypes;
    internal IReadOnlySet<PropertyInfo> ExcludeProperties => excludeProperties;

    internal IReadOnlyDictionary<PropertyInfo, IPropertyPrintingConfig<TOwner>> PropertyPrintingConfigs =>
        propertyPrintingConfigs;

    internal IReadOnlyDictionary<Type, ITypePrintingConfig<TOwner>> TypePrintingConfigs => typePrintingConfigs;


    public PrintingConfig<TOwner> UsingCommonCulture(CultureInfo culture)
    {
        CommonCulture = culture;
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
        var type = typeof(TType);
        if (typePrintingConfigs.TryGetValue(type, out var config)
            && config is TypePrintingConfig<TOwner, TType> printingConfig)
        {
            return printingConfig;
        }

        var typeConfig = new TypePrintingConfig<TOwner, TType>(this);
        typePrintingConfigs[type] = typeConfig;
        return typeConfig;
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var propertyInfo = ReflectionHelper.GetPropertyInfoFromExpression(memberSelector);
        if (propertyPrintingConfigs.TryGetValue(propertyInfo, out var config)
            && config is PropertyPrintingConfig<TOwner, TPropType> printingConfig)
        {
            return printingConfig;
        }

        var propertyPrintingConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
        propertyPrintingConfigs[propertyInfo] = propertyPrintingConfig;
        return propertyPrintingConfig;
    }

    public string PrintToString(TOwner obj) => objectPrinter.PrintToString(obj);
}