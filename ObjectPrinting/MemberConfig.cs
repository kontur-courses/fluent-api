using System;

namespace ObjectPrinting;

public sealed class MemberConfig<TOwner, TMember>
{
    private readonly PrintingConfig<TOwner> _parentConfig;
    private readonly MemberPath _member;

    internal MemberPath Member => _member;

    internal MemberConfig(PrintingConfig<TOwner> config, MemberPath member)
    {
        _parentConfig = config;
        _member = member;
    }

    public MemberConfig<TOwner, TMember> SetFormat(Func<TMember, string> format)
    {

        ArgumentNullException.ThrowIfNull(format, nameof(format));

        return WithNewRule((rules) => rules.AddMemberSerializationRule(_member, format));
    }

    public PrintingConfig<TOwner> Then() => _parentConfig;

    internal MemberConfig<TOwner, TMember> WithNewRule(Func<PrintingRules, PrintingRules> newRule)
    {
        var newRules = newRule(_parentConfig.Rules);
        var newConfig = new PrintingConfig<TOwner>(newRules);
        return new(newConfig, _member);
    }
}
