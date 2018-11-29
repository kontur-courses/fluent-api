using System;
using System.Globalization;

namespace ObjectPrinting.SerializingConfig
{
    public static class SerilalizingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this ISerializingConfig<TOwner, int> config, CultureInfo ci)
        {
            config.TypeOperations.Add(typeof(int), n => ((int)n).ToString(ci));
            return config.SerializingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this ISerializingConfig<TOwner, double> config, CultureInfo ci)
        {
            config.TypeOperations.Add(typeof(double), n => ((double)n).ToString(ci));
            return config.SerializingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this ISerializingConfig<TOwner, float> config, CultureInfo ci)
        {
            config.TypeOperations.Add(typeof(float), n => ((float)n).ToString(ci));
            return config.SerializingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this ISerializingConfig<TOwner, long> config, CultureInfo ci)
        {
            config.TypeOperations.Add(typeof(long), n => ((long)n).ToString(ci));
            return config.SerializingConfig;
        }

        public static PrintingConfig<TOwner> Cut<TOwner>(this ISerializingConfig<TOwner, string> config, int number)
        {
            config.TypeOperations.Add(typeof(string),
                l => number < ((string) l).Length
                    ? ((string) l).Substring(number)
                    : ((string) l).Substring(((string) l).Length - 1));
            return config.SerializingConfig;
        }
    }
}
