using System;
using System.Globalization;
using ObjectPrinting.Common.Configs;

namespace ObjectPrinting.Extensions
{
    public static class SerializingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this SerializingConfig<TOwner, TPropType> config, CultureInfo culture) where TPropType : IFormattable
        {
            return config.Using(x => x.ToString(null, culture));
        }

        public static PrintingConfig<TOwner> Trim<TOwner>(
            this SerializingConfig<TOwner, string> config, int maxLength)
        {
            return config.Using(x => x.Substring(0, maxLength > x.Length ? x.Length : maxLength));
        }
    }
}