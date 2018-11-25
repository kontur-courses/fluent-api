using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    public static class SerilalizingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this SerializingConfig<TOwner, int> config, CultureInfo ci)
        {
            return ((ISerializingConfig<TOwner>)config).SerializingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this SerializingConfig<TOwner, double> config, CultureInfo ci)
        {
            return ((ISerializingConfig<TOwner>)config).SerializingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this SerializingConfig<TOwner, float> config, CultureInfo ci)
        {
            return ((ISerializingConfig<TOwner>)config).SerializingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this SerializingConfig<TOwner, long> config, CultureInfo ci)
        {
            return ((ISerializingConfig<TOwner>)config).SerializingConfig;
        }

        public static PrintingConfig<TOwner> Cut<TOwner>(this SerializingConfig<TOwner, string> config, int number)
        {
            var printingConfig = ((ISerializingConfig<TOwner>)config).SerializingConfig;
            printingConfig.typeOperations.Add(typeof(string), new Func<string, string>(l => l.Substring(number)));
            return printingConfig;
        }
    }
}
