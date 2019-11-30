using System.Globalization;

namespace ObjectPrinting
{
    public static class SerializerConfigExtensions
    {
        public static PrintingConfig<TOwner> WithCulture<TOwner>(this SerializerConfig<TOwner, int> config, CultureInfo currentCulture)
        {
            (config as ISerializerConfig<TOwner>).SerializeFunc = x => ((int)x).ToString("N", currentCulture.NumberFormat);
            return (config as ISerializerConfig<TOwner>).ParentConfig;
        }
        
        public static PrintingConfig<TOwner> WithCulture<TOwner>(this SerializerConfig<TOwner, double> config, CultureInfo currentCulture)
        {
            (config as ISerializerConfig<TOwner>).SerializeFunc = x => ((double)x).ToString("N", currentCulture.NumberFormat);
            return (config as ISerializerConfig<TOwner>).ParentConfig;
        }
        
        public static PrintingConfig<TOwner> WithCulture<TOwner>(this SerializerConfig<TOwner, long> config, CultureInfo currentCulture)
        {
            (config as ISerializerConfig<TOwner>).SerializeFunc = x => ((long)x).ToString("N", currentCulture.NumberFormat);
            return (config as ISerializerConfig<TOwner>).ParentConfig;
        }
        
        public static PrintingConfig<TOwner> Cut<TOwner>(this SerializerConfig<TOwner, string> config, int countSymbols)
        {
            (config as ISerializerConfig<TOwner>).SerializeFunc = x => (x.ToString()).Substring(0, countSymbols);
            return (config as ISerializerConfig<TOwner>).ParentConfig;
        }
    }
}