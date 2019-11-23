using System.Globalization;
using ObjectPrinting.Configs;
using ObjectPrinting.Configs.ConfigInterfaces;

namespace ObjectPrinting.Extensions
{
    public static class PropertySerializationConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertySerializationConfig<TOwner, int> pc, 
            CultureInfo culture)
        {
            return (pc as IPropertySerializationConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Take<TOwner>(
            this PropertySerializationConfig<TOwner, string> pc,
            int count)
        {
            return (pc as IPropertySerializationConfig<TOwner>).ParentConfig;
        }
    }
}