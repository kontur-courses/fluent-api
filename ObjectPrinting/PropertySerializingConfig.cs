using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertySerializingConfig<TOwner, T> : IPropertySerializingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;

        public PropertySerializingConfig(PrintingConfig<TOwner> parentConfig)
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

    public static class PropertySerializingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializingConfig<TOwner, byte> config, CultureInfo cultureInfo)
        {
            config.Serializer = (v) => v.ToString("N", cultureInfo.NumberFormat);
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializingConfig<TOwner, short> config, CultureInfo cultureInfo)
        {
            config.Serializer = (v) => v.ToString("N", cultureInfo.NumberFormat);
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializingConfig<TOwner, int> config, CultureInfo cultureInfo)
        {
            config.Serializer = (v) => v.ToString("N", cultureInfo.NumberFormat);
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializingConfig<TOwner, long> config, CultureInfo cultureInfo)
        {
            config.Serializer = (v) => v.ToString("N", cultureInfo.NumberFormat);
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializingConfig<TOwner, float> config, CultureInfo cultureInfo)
        {
            config.Serializer = (v) => v.ToString("N", cultureInfo.NumberFormat);
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertySerializingConfig<TOwner, double> config, CultureInfo cultureInfo)
        {
            config.Serializer = (v) => v.ToString("N", cultureInfo.NumberFormat);
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> WithMaxLength<TOwner>(this PropertySerializingConfig<TOwner, string> config, ushort maxLength)
        {
            config.Serializer = (v) => maxLength <= v.Length ? v.Substring(0, maxLength) : v;
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }
    }
}
