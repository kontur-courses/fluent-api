using System.Globalization;

namespace ObjectPrinting;

internal interface IPropertyPrintingConfig<TOwner>
{
    PrintingConfig<TOwner> ParentConfig { get; }

    bool IsExcluded { get; }

    CultureInfo? CultureInfo { get; }

    Func<object, string>? PrintOverride { get; }
}
