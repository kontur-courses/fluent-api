using System;
using System.Globalization;

namespace ObjectPrinting.Extensions
{
    public static class PrintingPropertyConfigExtensions
    {
        public static PrintingConfig<TOwner> ToTrimmedLength<TOwner>(
            this PrintingPropertyConfig<TOwner, string> config, int maxLength) =>
            config.To(x => x.Length <= maxLength ? x : x[..maxLength]);

        public static PrintingConfig<TOwner> To<TOwner, TPropType>(
            this PrintingPropertyConfig<TOwner, TPropType> config, CultureInfo culture) where TPropType : IFormattable
        {
            return config.To(x => x.ToString(null, culture));
        }
    }
}
