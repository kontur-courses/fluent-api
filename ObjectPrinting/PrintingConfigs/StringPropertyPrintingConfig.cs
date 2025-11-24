using System;
using System.Linq.Expressions;

namespace ObjectPrinting.PrintingConfigs;

public class StringPropertyPrintingConfig<TOwner> : PropertyPrintingConfig<TOwner, string>
{
    public StringPropertyPrintingConfig(
        PrintingConfig<TOwner> config, Expression<Func<TOwner, string>> selector)
        : base(config, selector)
    {
    }

    public PrintingConfig<TOwner> TrimToLength(int maxLength)
    {
        return Config.TrimMember(Selector, maxLength);
    }
}