using System.Globalization;

namespace ObjectPrinting
{
    public static class PrintingUsingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this SerializingConfig<TOwner, int> config, CultureInfo ci)
        {
            return ((ISerializingConfig<TOwner>)config).SerializingConfig;
        }
        public static PrintingConfig<TOwner> Trim<TOwner>(this SerializingConfig<TOwner, string> config)
        {
            return ((ISerializingConfig<TOwner>)config).SerializingConfig;
        }
    }
}
