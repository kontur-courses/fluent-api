using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertySerializerConfig<TOwner, T> : IPropertySerializingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;

        public PropertySerializerConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        PrintingConfig<TOwner> IPropertySerializingConfig<TOwner>.ParentConfig => parentConfig;

        public Func<T, string> Serializer = null;
        public PrintingConfig<TOwner> Using(Func<T, string> func)
        {
            Serializer = func;
            return parentConfig;
        }

        public string Serialize(object property)
        {
            if (property is T typedProperty && Serializer != null)
                    return Serializer(typedProperty) + Environment.NewLine;
            else
                throw new ArgumentException();
        }
    }

    public static class PropertySerializingConfigExcentions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializerConfig<TOwner, byte> config, CultureInfo cultureInfo)
        {
            config.Serializer = (v) => v.ToString("N", cultureInfo.NumberFormat);
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializerConfig<TOwner, short> config, CultureInfo cultureInfo)
        {
            config.Serializer = (v) => v.ToString("N", cultureInfo.NumberFormat);
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializerConfig<TOwner, int> config, CultureInfo cultureInfo)
        {
            config.Serializer = (v) => v.ToString("N", cultureInfo.NumberFormat);
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializerConfig<TOwner, long> config, CultureInfo cultureInfo)
        {
            config.Serializer = (v) => v.ToString("N", cultureInfo.NumberFormat);
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializerConfig<TOwner, float> config, CultureInfo cultureInfo)
        {
            config.Serializer = (v) => v.ToString("N", cultureInfo.NumberFormat);
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializerConfig<TOwner, double> config, CultureInfo cultureInfo)
        {
            config.Serializer = (v) => v.ToString("N", cultureInfo.NumberFormat);
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> WithMaxLength<TOwner>(this PropertySerializerConfig<TOwner, string> config, ushort maxLength)
        {
            config.Serializer = (v) => maxLength <= v.Length ? v.Substring(0, maxLength) : v;
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }
    }
}
