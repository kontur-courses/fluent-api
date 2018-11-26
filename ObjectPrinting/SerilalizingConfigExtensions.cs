using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class SerilalizingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this SerializingConfig<TOwner, int> config, CultureInfo ci)
        {
            var printingConfig = ((ISerializingConfig<TOwner, int>)config).SerializingConfig;
            printingConfig.AddTypeOperation(typeof(int), new Func<int, string>(l => l.ToString(ci)));
            return printingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this SerializingConfig<TOwner, double> config, CultureInfo ci)
        {
            var printingConfig = ((ISerializingConfig<TOwner, double>)config).SerializingConfig;
            printingConfig.AddTypeOperation(typeof(double), new Func<double, string>(l => l.ToString(ci)));
            return printingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this SerializingConfig<TOwner, float> config, CultureInfo ci)
        {
            var printingConfig = ((ISerializingConfig<TOwner, float>)config).SerializingConfig;
            printingConfig.AddTypeOperation(typeof(float), new Func<float, string>(l => l.ToString(ci)));
            return printingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this SerializingConfig<TOwner, long> config, CultureInfo ci)
        {
            var printingConfig = ((ISerializingConfig<TOwner, long>)config).SerializingConfig;
            printingConfig.AddTypeOperation(typeof(long), new Func<long, string>(l => l.ToString(ci)));
            return printingConfig;
        }

        public static PrintingConfig<TOwner> Cut<TOwner>(this SerializingConfig<TOwner, string> config, int number)
        {
            var printingConfig = ((ISerializingConfig<TOwner, string>)config).SerializingConfig;

            printingConfig.AddTypeOperation(typeof(string), new Func<string, string>(l => number < l.Length ? l.Substring(number) : l.Substring(l.Length - 1)));
            return printingConfig;
        }
    }
}
