using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertySerializingConfigExtensions
    {
        public static PrintingConfig<TOwner> WithCulture<TOwner, TPropType>
            (this PropertySerializationConfig<TOwner, TPropType> config, CultureInfo culture)
            where TPropType : IFormattable
        {
            ((config as IPropertySerializingConfig<TOwner>).ParentConfig as IPrintingConfig)
                .SerializationInfo.AddCultureRule(typeof(TPropType), culture);

            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }


        public static PrintingConfig<TOwner> Trim<TOwner>(this PropertySerializationConfig<TOwner, string> config,
            int length)
        {
            ((config as IPropertySerializingConfig<TOwner>).ParentConfig as IPrintingConfig)
                .SerializationInfo.AddTrimRule((config as IPropertySerializingConfig<TOwner>).CurrentName, length);

            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }
    }
}