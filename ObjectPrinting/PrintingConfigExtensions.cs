using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting;

public static class PrintingConfigExtensions
{
    public static PrintingConfig<TOwner> ExcludeType<TOwner>(this PrintingConfig<TOwner> config, Type typeToExclude)
    {
        ArgumentNullException.ThrowIfNull(typeToExclude);
        config.Settings.ExcludeType(typeToExclude);
        return config;
    }

    public static PrintingConfig<TOwner> ExcludeType<TOwner>(
        this PrintingConfig<TOwner> config,
        IEnumerable<Type> typesToExclude)
    {
        ArgumentNullException.ThrowIfNull(typesToExclude);
        foreach (var typeToExclude in typesToExclude)
        {
            config.ExcludeType(typeToExclude);
        }

        return config;
    }

    public static PrintingConfig<TOwner> ExcludeMember<TOwner>(this PrintingConfig<TOwner> config, string memberName)
    {
        ArgumentNullException.ThrowIfNull(memberName);
        var members = typeof(TOwner).GetMember(
            memberName,
            MemberTypes.Property | MemberTypes.Field,
            BindingFlags.Public | BindingFlags.Instance);
        ThrowIfMemberNotFound<TOwner>(members, memberName);
        config.Settings.ExcludeMember(members.Single());
        return config;
    }

    public static PrintingConfig<TOwner> ExcludeMember<TOwner>(
        this PrintingConfig<TOwner> config,
        IEnumerable<string> memberNames)
    {
        ArgumentNullException.ThrowIfNull(memberNames);
        foreach (var memberName in memberNames)
        {
            config.ExcludeMember(memberName);
        }

        return config;
    }

    public static PrintingConfig<TOwner> Using<TOwner, TPrintingType>(
        this ValuePrintingConfig<TOwner, TPrintingType> config,
        CultureInfo culture)
        where TPrintingType : IFormattable
    {
        ArgumentNullException.ThrowIfNull(culture);
        return config.Using(value => value.ToString(null, culture));
    }

    public static PrintingConfig<TOwner> Using<TOwner, TPrintingType>(
        this ValuePrintingConfig<TOwner, TPrintingType> config,
        string format)
        where TPrintingType : IFormattable
    {
        ArgumentNullException.ThrowIfNull(format);
        return config.Using(value => value.ToString(format, null));
    }

    public static PrintingConfig<TOwner> Using<TOwner, TPrintingType>(
        this ValuePrintingConfig<TOwner, TPrintingType> config,
        string format,
        CultureInfo culture)
        where TPrintingType : IFormattable
    {
        ArgumentNullException.ThrowIfNull(format);
        ArgumentNullException.ThrowIfNull(culture);
        return config.Using(value => value.ToString(format, culture));
    }

    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this ValuePrintingConfig<TOwner, string> config,
        int maxLength)
    {
        return config.TrimmedToLength(maxLength, string.Empty);
    }

    public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
        this ValuePrintingConfig<TOwner, string> config,
        int maxLength,
        string ending)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);
        ArgumentNullException.ThrowIfNull(ending);
        return config.Using(value => value.Length <= maxLength ? value : value[..maxLength] + ending);
    }

    private static void ThrowIfMemberNotFound<TOwner>(MemberInfo[] members, string memberName)
    {
        if (members.Length > 0)
            return;

        throw new ArgumentException(
            $"{typeof(TOwner).Name} has no public property or field named {memberName}",
            nameof(memberName));
    }
}