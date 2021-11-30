using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TType>
            (this TypePrintingConfig<TOwner, TType> config, CultureInfo culture)
            where TType : IFormattable
        {
            config.MemberConfigs
                .ForEach(c => c.Using(culture));
            return config.ParentConfig;
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>
            (this TypePrintingConfig<TOwner, string> config, int length)
        {
            config.MemberConfigs
                .ForEach(c => c.TrimmedToLength(length));
            return config.ParentConfig;
        }
    }
}