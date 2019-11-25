using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertySerializingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, T>(this PropertySerializingConfig<TOwner, T> config, CultureInfo cultureInfo) where T : IFormattable
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
