using System;
using System.Globalization;
using ObjectPrintingTask.PrintingConfiguration;

namespace ObjectPrintingTask.Extensions
{
    public static class ChildPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TMemberType>(
            this IChildPrintingConfig<TOwner, TMemberType> config,
            CultureInfo cultureInfo = null,
            string format = null) where TMemberType : IFormattable
        {
            return config.Using(obj => obj.ToString(format, cultureInfo));
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this IChildPrintingConfig<TOwner, string> config, int maxLen)
        {
            if (maxLen < 0)
                throw new ArgumentException("Cut amount should be >= 0");

            return config.Using(str => str.Substring(0, Math.Min(str.Length, maxLen)));
        }
    }
}