﻿using System;
using System.Globalization;

namespace ObjectPrinting.Extensions;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
        this PropertyPrintingConfig<TOwner, TPropType> config, CultureInfo culture) where TPropType : IFormattable
    {
        return config.Using(x => x.ToString(null, culture));
    }

    public static PrintingConfig<TOwner> Trimed<TOwner>(
        this PropertyPrintingConfig<TOwner, string> config, int maxLength)
    {
        return config.Using(x => x[..Math.Min(x.Length, maxLength)]);
    }
}