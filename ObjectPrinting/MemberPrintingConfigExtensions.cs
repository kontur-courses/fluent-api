using System;

namespace ObjectPrinting;

public static class MemberPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> WithMaxLength<TOwner>(
        this MemberPrintingConfig<TOwner, string> cfg,
        int maxLength)
    {
        cfg.Using(s => s[..Math.Min(s.Length, maxLength)]);
        return ((IMemberPrintingConfig<TOwner>) cfg).PrintingConfig;
    }
}