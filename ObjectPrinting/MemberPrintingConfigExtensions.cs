using System;
using System.Globalization;
using ObjectPrinting.Abstractions.Configs;

namespace ObjectPrinting;

public static class MemberPrintingConfigExtensions
{
    public static IPrintingConfig<TOwner> WithMaxLength<TOwner>(
        this IMemberPrintingConfig<TOwner, string> cfg,
        int maxLength)
    {
        if (maxLength < 0)
            throw new ArgumentException($"{nameof(maxLength)} cannot be less than zero!");
        return cfg.Using(s => s[..Math.Min(s.Length, maxLength)]);
    }

    public static IPrintingConfig<TOwner> WithCulture<TOwner>(
        this IMemberPrintingConfig<TOwner, float> cfg,
        CultureInfo culture)
    {
        return cfg.Using(f => f.ToString(culture));
    }

    public static IPrintingConfig<TOwner> WithCulture<TOwner>(
        this IMemberPrintingConfig<TOwner, double> cfg,
        CultureInfo culture)
    {
        return cfg.Using(d => d.ToString(culture));
    }

    public static IPrintingConfig<TOwner> WithCulture<TOwner>(
        this IMemberPrintingConfig<TOwner, DateTime> cfg,
        CultureInfo culture)
    {
        return cfg.Using(dt => dt.ToString(culture));
    }

    public static IPrintingConfig<TOwner> WithRounding<TOwner>(
        this IMemberPrintingConfig<TOwner, float> cfg,
        int decimalPartLength)
    {
        if (decimalPartLength < 0)
            throw new ArgumentException($"{nameof(decimalPartLength)} cannot be less than zero!");
        return cfg.Using(f => Math.Round(f, decimalPartLength).ToString(CultureInfo.CurrentCulture));
    }

    public static IPrintingConfig<TOwner> WithRounding<TOwner>(
        this IMemberPrintingConfig<TOwner, double> cfg,
        int decimalPartLength)
    {
        if (decimalPartLength < 0)
            throw new ArgumentException($"{nameof(decimalPartLength)} cannot be less than zero!");
        return cfg.Using(d => Math.Round(d, decimalPartLength).ToString(CultureInfo.CurrentCulture));
    }
}