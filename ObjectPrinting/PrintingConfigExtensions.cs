using System;

namespace ObjectPrinting
{
    public static class PrintingConfigExtensions {
        public static PrintingConfig<TOwner> Use<TOwner, TType>(
            this PrintingConfig<TOwner>.NestingPrintingConfig<TType> nestingConfig, IFormatProvider provider)
            where TType : IFormattable
        {
            return nestingConfig.Use(value => value.ToString(null, provider));
        }
    }
}