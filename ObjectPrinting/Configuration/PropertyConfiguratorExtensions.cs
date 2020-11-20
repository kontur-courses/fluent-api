using System;
using System.Globalization;
using ObjectPrinting.Serializers;

namespace ObjectPrinting.Configuration
{
    public static class PropertyConfiguratorExtensions
    {
        public static PrintingConfig<TOwner> Exclude<TOwner, TProperty>(this IConfigurable<TOwner, TProperty> property)
        {
            return property.Using(new IgnorePropertySerializer<TProperty>());
        }

        public static PrintingConfig<TOwner> UseSerializer<TOwner, TProperty>(
            this IConfigurable<TOwner, TProperty> property,
            Func<TProperty, string> serializer)
        {
            return property.Using(new DelegatePropertySerializer<TProperty>(serializer));
        }

        public static PrintingConfig<TOwner> Trim<TOwner>(this IConfigurable<TOwner, string> property,
            int maxLength)
        {
            return property.Using(new TrimmedPropertySerializer(maxLength));
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner, TProperty>(
            this IConfigurable<TOwner, TProperty> property, string format, CultureInfo cultureInfo)
            where TProperty : IFormattable
        {
            return property.Using(new CultureSpecifiedPropertySerializer<TProperty>(cultureInfo, format));
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner, TProperty>(
            this IConfigurable<TOwner, TProperty> property, CultureInfo cultureInfo)
            where TProperty : IFormattable
        {
            return property.Using(new CultureSpecifiedPropertySerializer<TProperty>(cultureInfo, null));
        }
    }
}