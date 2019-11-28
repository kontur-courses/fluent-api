using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertySerializingConfigExtensions
    {
        public static PrintingConfig<TOwner> WithCulture<TOwner>
            (this PropertySerializationConfig<TOwner, double> config, CultureInfo culture)
        {
            (config as IPrintingConfig)?.SerializationInfo.AddCultureRule(typeof(double), culture);
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Trim<TOwner>(this PropertySerializationConfig<TOwner, string> config,
            int length)
        {
            (config as IPrintingConfig)?.SerializationInfo.AddTrimRule(
                (config as IPropertySerializingConfig<TOwner>).PropInfo, length);
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }
    }
}