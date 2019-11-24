using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using ObjectPrinting.Configs;
using ObjectPrinting.Configs.ConfigInterfaces;

namespace ObjectPrinting.Extensions
{
    public static class PropertySerializationConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertySerializationConfig<TOwner, double> propertySerializationConfig, 
            CultureInfo culture)
        {
            var castedPropertySerialization = propertySerializationConfig as IPropertySerializationConfig<TOwner>;
            var propertyInfo = castedPropertySerialization.PropertyInfo;
            var parentConfig = castedPropertySerialization.ParentConfig;
            var casted = (IPrintingConfig<TOwner>) parentConfig;
            Func<object, string> serialization = o => Convert.ToString((double) o, culture);
            if (propertyInfo != null) return casted.AddPropertySerialization(propertyInfo, serialization);
            return casted.AddTypeSerialization(typeof(double), serialization);
        }

        public static PrintingConfig<TOwner> Take<TOwner>(
            this PropertySerializationConfig<TOwner, string> propertySerializationConfig,
            int count)
        {
            var castedPropertySerialization = propertySerializationConfig as IPropertySerializationConfig<TOwner>;
            var propertyInfo = castedPropertySerialization.PropertyInfo;
            var parentConfig = castedPropertySerialization.ParentConfig;
            var casted = (IPrintingConfig<TOwner>) parentConfig;
            Func<object, string> serialization = o => o.ToString().Substring(0, count);
            if (propertyInfo != null) return casted.AddPropertySerialization(propertyInfo, serialization);
            return casted.AddTypeSerialization(typeof(double), serialization);
        }
    }
}