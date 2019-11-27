using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class SerializerConfig<TOwner, T> : ISerializerConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private Func<object, string> propertySerializeFunc;

        public SerializerConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        PrintingConfig<TOwner> ISerializerConfig<TOwner>.ParentConfig => parentConfig;

        Func<object, string> ISerializerConfig<TOwner>.SerializeFunc
        {
            get => propertySerializeFunc;
            set => propertySerializeFunc = value;
        }

        public PrintingConfig<TOwner> Using(Func<T, string> func)
        {
            propertySerializeFunc = x => func((T)x);
            return parentConfig;
        }
    }

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