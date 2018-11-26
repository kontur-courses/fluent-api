using System;
using System.Globalization;

namespace ObjectPrinting.SerializingConfig
{
    public static class SerilalizingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this ISerializingConfig<TOwner, int> config, CultureInfo ci)
        {
            var printingConfig = config.SerializingConfig;
            printingConfig.typeOperations.Add(typeof(int), new Func<int, string>(l => l.ToString(ci)));
            return printingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this ISerializingConfig<TOwner, double> config, CultureInfo ci)
        {
            var printingConfig = config.SerializingConfig;
            printingConfig.typeOperations.Add(typeof(double), new Func<double, string>(l => l.ToString(ci)));
            return printingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this ISerializingConfig<TOwner, float> config, CultureInfo ci)
        {
            var printingConfig = config.SerializingConfig;
            printingConfig.typeOperations.Add(typeof(float), new Func<float, string>(l => l.ToString(ci)));
            return printingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this ISerializingConfig<TOwner, long> config, CultureInfo ci)
        {
            var printingConfig = config.SerializingConfig;
            printingConfig.typeOperations.Add(typeof(long), new Func<long, string>(l => l.ToString(ci)));
            return printingConfig;
        }

        public static PrintingConfig<TOwner> Cut<TOwner>(this ISerializingConfig<TOwner, string> config, int number)
        {
            var printingConfig = config.SerializingConfig;
            printingConfig.typeOperations.Add(typeof(string), new Func<string, string>(l => number < l.Length ? l.Substring(number) : l.Substring(l.Length - 1)));
            return printingConfig;
        }
    }
}
