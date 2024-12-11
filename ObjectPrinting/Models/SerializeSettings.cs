using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.Models;

internal class SerializeSettings
{
    public CultureInfo CommonCulture { get; init; }
    public IReadOnlySet<Type> ExcludeTypes { get; init; }
    public IReadOnlySet<PropertyInfo> ExcludeProperties { get; init; }
    public IReadOnlyDictionary<PropertyInfo, Delegate> PropertyPrintingConfigs { get; init; }
    public IReadOnlyDictionary<Type, Delegate> TypePrintingConfigs { get; init; }
    public IReadOnlyDictionary<Type, CultureInfo> TypeCultureConfigs { get; init; }

    public SerializeSettings(CultureInfo commonCulture, IEnumerable<Type> excludeTypes,
        IEnumerable<PropertyInfo> excludeProperties,
        IDictionary<PropertyInfo, Delegate> propertyPrintingConfigs, IDictionary<Type, Delegate> typePrintingConfigs,
        IDictionary<Type, CultureInfo> typeCultureConfigs)
    {
        CommonCulture = commonCulture;
        ExcludeTypes = excludeTypes.ToFrozenSet();
        ExcludeProperties = excludeProperties.ToFrozenSet();
        PropertyPrintingConfigs = propertyPrintingConfigs.ToFrozenDictionary();
        TypePrintingConfigs = typePrintingConfigs.ToFrozenDictionary();
        TypeCultureConfigs = typeCultureConfigs.ToFrozenDictionary();
    }
}