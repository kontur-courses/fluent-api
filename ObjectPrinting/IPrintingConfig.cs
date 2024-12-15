using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ObjectPrinting;

internal interface IPrintingConfig<TOwner>
{
    Dictionary<Type, IPropertyPrintingConfig<TOwner>> PropertyConfigsByType { get; }

    Dictionary<PropertyPath, IPropertyPrintingConfig<TOwner>> PropertyConfigsByPath { get; }

    CultureInfo CultureInfo { get; }

    bool IsToLimitNestingLevel { get; }

    int MaxNestingLevel { get; }

    public bool TryGetConfig(
        PropertyPath path,
        [MaybeNullWhen(false)] out IPropertyPrintingConfig<TOwner> propertyPrintingConfig)
    {
        var obj = path.PropertyValue.Value;
        return PropertyConfigsByPath.TryGetValue(path, out propertyPrintingConfig)
            || obj != null
                && PropertyConfigsByType.TryGetValue(obj.GetType(), out propertyPrintingConfig);
    }
}
