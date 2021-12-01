using System;
using System.Globalization;

namespace ObjectPrinting.Extensions
{
    public static class MemberPrintingExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TMember>(this MemberPrintingConfig<TOwner, TMember> config,
            CultureInfo culture, string format = null)
            where TMember : IFormattable
        {
            return config.Using(o => o.ToString(format, culture));
        }
    }
}