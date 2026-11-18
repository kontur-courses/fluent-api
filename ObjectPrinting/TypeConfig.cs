using System;

namespace ObjectPrinting;

public sealed class TypeConfig<TOwner, TType>
{
    private readonly PrintingConfig<TOwner> _parentConfig;

    internal TypeConfig(PrintingConfig<TOwner> printingConfig)
    {
        _parentConfig = printingConfig;
    }

    public TypeConfig<TOwner, TType> SetFormat(Func<TType, string> format)
    {
        ArgumentNullException.ThrowIfNull(format, nameof(format));

        return WithNewRule((rules) => rules.AddTypeSerializationRule(format));
    }

    public PrintingConfig<TOwner> Then() => _parentConfig;

    internal TypeConfig<TOwner, TType> WithNewRule(Func<PrintingRules, PrintingRules> newRule)
    {
        var newRules = newRule(_parentConfig.Rules);
        var newConfig = new PrintingConfig<TOwner>(newRules);

        return new(newConfig);
    }
}
