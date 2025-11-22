using System;
using System.Numerics;
using ObjectPrinting.Configs.Interfaces;

namespace ObjectPrinting.Configs.Extensions;

public static class TypePrintingConfigExtensions
{
    public static PrintingConfig<TOwner> Trim<TOwner>(
        this TypePrintingConfig<TOwner, string> config,
        int maxLen)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLen);

        var parent = ((IChildPrintingConfig<TOwner, string>)config).ParentConfig;

        parent.Settings.GlobalStringTrimLength = maxLen;

        return parent;
    }

    public static PrintingConfig<TOwner> Use<TOwner, TPropType>(
        this TypePrintingConfig<TOwner, TPropType> config,
        IFormatProvider provider) where TPropType : IFormattable
    {
        ArgumentNullException.ThrowIfNull(provider);

        var parent = ((IChildPrintingConfig<TOwner, TPropType>)config).ParentConfig;

        parent.Settings.TypeCultures[typeof(TPropType)] = provider;

        return parent;
    }
}