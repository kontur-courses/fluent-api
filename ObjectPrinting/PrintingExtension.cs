using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PrintingExtension
    {
        public static PrintingConfig<TOwner> Truncate<TOwner>(
            this PropertyConfig<TOwner, string> config,
            int startPos,
            int length
        )
        {
            return config.SerializeAs(s => s.Substring(startPos, length));
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner, TResult>(
            this TypeConfig<TOwner, TResult> config,
            CultureInfo info
        ) where TResult : IFormattable
        {
            return config.SerializeAs(obj => obj.ToString("", info));
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner, TResult>(
            this PropertyConfig<TOwner, TResult> config,
            CultureInfo info
        ) where TResult : IFormattable
        {
            return config.SerializeAs(obj => obj.ToString("", info));
        }
    }
}