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
        ThrowIfLessThanZero(maxLength, nameof(maxLength));
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
        ThrowIfLessThanZero(decimalPartLength, nameof(decimalPartLength));
        return cfg.Using(f => RoundToString(f, decimalPartLength));
    }

    public static IPrintingConfig<TOwner> WithRounding<TOwner>(
        this IMemberPrintingConfig<TOwner, double> cfg,
        int decimalPartLength)
    {
        ThrowIfLessThanZero(decimalPartLength, nameof(decimalPartLength));
        return cfg.Using(d => RoundToString(d, decimalPartLength));
    }

    private static string RoundToString(double value, int decimalPartLength) =>
        Math.Round(value, decimalPartLength).ToString(CultureInfo.CurrentCulture);

    private static void ThrowIfLessThanZero(int value, string argName)
    {
        if (value < 0)
            throw new ArgumentException($"{argName} cannot be less than zero!");
    }
}