using System;
using System.Globalization;

namespace ObjectPrinting;

public static class MemberPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> WithMaxLength<TOwner>(
        this MemberPrintingConfig<TOwner, string> cfg,
        int maxLength)
    {
        if (maxLength < 0)
            throw new ArgumentException($"{nameof(maxLength)} cannot be less than zero!");
        cfg.Using(s => s[..Math.Min(s.Length, maxLength)]);
        return ((IMemberPrintingConfig<TOwner>) cfg).PrintingConfig;
    }

    public static PrintingConfig<TOwner> WithCulture<TOwner>(
        this MemberPrintingConfig<TOwner, float> cfg,
        CultureInfo culture)
    {
        cfg.Using(f => f.ToString(culture));
        return ((IMemberPrintingConfig<TOwner>) cfg).PrintingConfig;
    }

    public static PrintingConfig<TOwner> WithCulture<TOwner>(
        this MemberPrintingConfig<TOwner, double> cfg,
        CultureInfo culture)
    {
        cfg.Using(d => d.ToString(culture));
        return ((IMemberPrintingConfig<TOwner>) cfg).PrintingConfig;
    }

    public static PrintingConfig<TOwner> WithCulture<TOwner>(
        this MemberPrintingConfig<TOwner, DateTime> cfg,
        CultureInfo culture)
    {
        cfg.Using(dt => dt.ToString(culture));
        return ((IMemberPrintingConfig<TOwner>) cfg).PrintingConfig;
    }

    public static PrintingConfig<TOwner> WithRounding<TOwner>(
        this MemberPrintingConfig<TOwner, float> cfg,
        int decimalPartLength)
    {
        if (decimalPartLength < 0)
            throw new ArgumentException($"{nameof(decimalPartLength)} cannot be less than zero!");
        cfg.Using(f => Math.Round(f, decimalPartLength).ToString(CultureInfo.CurrentCulture));
        return ((IMemberPrintingConfig<TOwner>) cfg).PrintingConfig;
    }

    public static PrintingConfig<TOwner> WithRounding<TOwner>(
        this MemberPrintingConfig<TOwner, double> cfg,
        int decimalPartLength)
    {
        if (decimalPartLength < 0)
            throw new ArgumentException($"{nameof(decimalPartLength)} cannot be less than zero!");
        cfg.Using(d => Math.Round(d, decimalPartLength).ToString(CultureInfo.CurrentCulture));
        return ((IMemberPrintingConfig<TOwner>) cfg).PrintingConfig;
    }
}