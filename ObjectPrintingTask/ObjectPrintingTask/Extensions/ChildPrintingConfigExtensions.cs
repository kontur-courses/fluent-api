using System;
using System.Globalization;
using ObjectPrintingTask.PrintingConfiguration;

namespace ObjectPrintingTask.Extensions
{
    public static class ChildPrintingConfigExtensions
    {
        public static Printer<TOwner> Using<TOwner, TMemberType>(
            this IChildPrintingConfig<TOwner, TMemberType> config,
            CultureInfo cultureInfo = null,
            string format = null) where TMemberType : IFormattable
        {
            if (cultureInfo == null)
                cultureInfo = CultureInfo.InvariantCulture;

            return config.Using(obj => obj.ToString(format, cultureInfo));
        }

        public static Printer<TOwner> TrimmedToLength<TOwner>(
            this IChildPrintingConfig<TOwner, string> config, int maxLen)
        {
            if (maxLen <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxLen), "Cut amount should be greater then 0");

            return config.Using(str =>
            {
                if (str == null)
                    return "null";

                return str.Substring(0, Math.Min(str.Length, maxLen));
            });
        }
    }
}