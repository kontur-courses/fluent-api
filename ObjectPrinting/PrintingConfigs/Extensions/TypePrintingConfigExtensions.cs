using System;
using System.Globalization;

namespace ObjectPrinting.PrintingConfigs.Extensions;

public static class TypePrintingConfigExtensions
{
    public static PrintingConfig<TOwner> Using<TOwner, TProp>(
        this TypePrintingConfig<TOwner, TProp> config, CultureInfo cultureInfo)
        where TProp : IFormattable
    {
        return config.Config.SetCultureFor<TProp>(cultureInfo);
    }
}