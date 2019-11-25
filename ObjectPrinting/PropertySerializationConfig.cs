using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertySerializationConfig<TOwner, TPropType> : IPropertySerializingConfig<TOwner>
    {
        private PrintingConfig<TOwner> parentConfig;
        PrintingConfig<TOwner> IPropertySerializingConfig<TOwner>.ParentConfig => parentConfig;

        public PropertySerializationConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;

        }

        public PrintingConfig<TOwner> WithSerialization(Func<TPropType, string> serializationFunc)
        {
            return parentConfig;
        }
    }

    public interface IPropertySerializingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }

    public static class PropertySerializingConfigExtensions
    {
        public static PrintingConfig<TOwner> WithCulture<TOwner>
            (this PropertySerializationConfig<TOwner, int> config, CultureInfo culture)
        {
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }


        public static PrintingConfig<TOwner> Trim<TOwner>(this PropertySerializationConfig<TOwner, string> config,
            int length)
        {
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }
    }
}