using System;
using System.Globalization;
using ObjectPrinting.Serializer.Configs.Children;

namespace ObjectPrinting.Serializer.Configs.Tools;

public static class ChildConfigExtensions
{
    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this IChildConfig<TOwner, string> childConfig, uint maxLen)
    {
        var parent = childConfig.ParentConfig;
        switch (childConfig)
        {
            case TypeConfig<TOwner, string>:
                parent.TrimStringValue = maxLen;
                return parent;
            
            case PropertyConfig<TOwner, string> propConfig:
                parent.TrimmedMembers[propConfig.PropertyName] = maxLen;
                return parent;
            
            default: throw new ArgumentException("Invalid child config type");
        }
    }

    public static PrintingConfig<TOwner> WithCulture<TOwner, TPropType>(
        this TypeConfig<TOwner, TPropType> childConfig, CultureInfo culture) where TPropType : IFormattable
    {
        childConfig.ParentConfig.CulturesForTypes[typeof(TPropType)] = culture;
        return childConfig.ParentConfig;
    }
}