using System;
using System.Globalization;

namespace ObjectPrinting.Extensions
{
    public static class MemberPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this MemberPrintingConfig<TOwner, string> memberConfig, int maxLength)
        {
            if (maxLength < 0)
                throw new ArgumentException($"{nameof(maxLength)} must be a positive number");

            return memberConfig.Using(str => str.Substring(0, Math.Min(str.Length, maxLength)));
        }

        public static PrintingConfig<TOwner> Using<TOwner, TMemberType>(
            this MemberPrintingConfig<TOwner, TMemberType> memberConfig, CultureInfo cultureInfo)
            where TMemberType : IFormattable
        {
            return memberConfig.Using(x => x.ToString(null, cultureInfo));
        }
    }
}