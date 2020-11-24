using System;
using System.Globalization;
using ObjectPrinting.Core;

namespace ObjectPrinting.Extensions
{
    public static class MemberPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this MemberPrintingConfig<TOwner, string> propConfig, int maxLength)
        {
            throw new NotImplementedException();
        }

        public static PrintingConfig<TOwner> SpecifyCulture<TOwner, T>(
            this MemberPrintingConfig<TOwner, T> memberConfig, CultureInfo culture) where T : IFormattable
        {
            throw new NotImplementedException();
        }
    }
}