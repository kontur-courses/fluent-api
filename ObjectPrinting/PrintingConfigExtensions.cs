using System;

namespace ObjectPrinting
{
    public static class PrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Use<TOwner, TType>(
            this INestingPrintingConfig<TOwner, TType> nestingConfig, IFormatProvider provider)
            where TType : IFormattable
        {
            return nestingConfig.Use(value => value.ToString(null, provider));
        }

        public static PrintingConfig<TOwner> UseSubstring<TOwner>(
            this INestingPrintingConfig<TOwner, string> nestingConfig, Range range)
        {
            return nestingConfig.Use(value => value[range]);
        }
    }
}