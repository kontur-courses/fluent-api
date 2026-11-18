using System;
using System.Globalization;

namespace ObjectPrinting;

public static class ConfigExtensions
{
    public static TypeConfig<TOwner, TType> SetCulture<TOwner, TType>(this TypeConfig<TOwner, TType> config,
                                                                        CultureInfo culture) where TType : IFormattable
    {
        ArgumentNullException.ThrowIfNull(culture, nameof(culture));

        return config.WithNewRule(rule => rule.AddTypeCultureRule(typeof(TType), culture));
    }

    public static MemberConfig<TOwner, TMember> SetCulture<TOwner, TMember>(this MemberConfig<TOwner, TMember> config,
                                                                              CultureInfo culture) where TMember : IFormattable
    {
        ArgumentNullException.ThrowIfNull(culture, nameof(culture));

        return config.WithNewRule(r => r.AddMemberCultureRule(config.Member, culture));
    }

    public static TypeConfig<TOwner, string> Truncate<TOwner>(this TypeConfig<TOwner, string> config,
                                                                int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length, nameof(length));

        return config.WithNewRule(r => r.AddTypeTruncateRule(typeof(string), length));
    }

    public static MemberConfig<TOwner, string> Truncate<TOwner>(this MemberConfig<TOwner, string> config,
                                                                  int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length, nameof(length));

        return config.WithNewRule(r => r.AddMemberTruncateRule(config.Member, length));
    }
}