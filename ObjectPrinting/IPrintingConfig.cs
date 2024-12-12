using System.Globalization;

namespace ObjectPrinting;

internal interface IPrintingConfig<TOwner>
{
    Dictionary<Type, IPropertyPrintingConfig<TOwner>> PropertyConfigsByType { get; }

    Dictionary<PropertyPath, IPropertyPrintingConfig<TOwner>> PropertyConfigsByPath { get; }

    CultureInfo CultureInfo { get; }

    bool IsToLimitNestingLevel { get; }

    int MaxNestingLevel { get; }
}
