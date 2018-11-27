using System;
using System.Globalization;

namespace ObjectPrinting.SerializingConfig
{
    public static class SerilalizingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this ISerializingConfig<TOwner, int> config, CultureInfo ci)
        {
            config.TypeOperations.Add(typeof(int), new Func<int, string>(l => l.ToString(ci)));
            return config.SerializingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this ISerializingConfig<TOwner, double> config, CultureInfo ci)
        {
            config.TypeOperations.Add(typeof(double), new Func<double, string>(l => l.ToString(ci)));
            return config.SerializingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this ISerializingConfig<TOwner, float> config, CultureInfo ci)
        {
            config.TypeOperations.Add(typeof(float), new Func<float, string>(l => l.ToString(ci)));
            return config.SerializingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this ISerializingConfig<TOwner, long> config, CultureInfo ci)
        {
            config.TypeOperations.Add(typeof(long), new Func<long, string>(l => l.ToString(ci)));
            return config.SerializingConfig;
        }

        public static PrintingConfig<TOwner> Cut<TOwner>(this ISerializingConfig<TOwner, string> config, int number)
        {
            config.TypeOperations.Add(typeof(string), new Func<string, string>(l => number < l.Length ? l.Substring(number) : l.Substring(l.Length - 1)));
            return config.SerializingConfig;
        }
    }
}
