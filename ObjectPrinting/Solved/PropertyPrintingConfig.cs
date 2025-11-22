using System;
using System.Globalization;

namespace ObjectPrinting.Solved;

public class PropertyPrintingConfig<TOwner, TPropType>(
    PrintingConfig<TOwner> printingConfig,
    string? memberName = null)
    : IPropertyPrintingConfig<TOwner, TPropType>
{
    public string? MemberName { get; } = memberName;

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        if (MemberName != null)
        {
            printingConfig.AddMemberSerializer(MemberName, print);
        }
        else
        {
            printingConfig.AddTypeSerializer(print);
        }

        return printingConfig;
    }

    public PrintingConfig<TOwner> Using(CultureInfo culture)
    {
        printingConfig.AddCulture<TPropType>(culture);
        return printingConfig;
    }

    public PrintingConfig<TOwner> ParentConfig => printingConfig;
}

public interface IPropertyPrintingConfig<TOwner, TPropType>
{
    PrintingConfig<TOwner> ParentConfig { get; }
}