using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PrintingUsingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this SerializingConfig<TOwner, int> config, CultureInfo cultureInfo)
        {
            return ((ISerializingConfig<TOwner>) config).SerializingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this SerializingConfig<TOwner, double> config, CultureInfo cultureInfo)
        {
            return ((ISerializingConfig<TOwner>) config).SerializingConfig;
        }

        public static PrintingConfig<TOwner> Cut<TOwner>(this SerializingConfig<TOwner, string> config, int strLen)
        {
            return ((ISerializingConfig<TOwner>) config).SerializingConfig;
        }
    }
}