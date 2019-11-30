using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace ObjectPrinting
{
    public static class PropertySerializingConfigExtension
    {
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertySerializingConfig<TOwner, double> propertySerializingConfig, CultureInfo cultureInfo)
        {
            return propertySerializingConfig.Using(d => d.ToString(cultureInfo) );
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertySerializingConfig<TOwner, int> propertySerializingConfig, CultureInfo cultureInfo)
        {
            return propertySerializingConfig.Using(d => d.ToString(cultureInfo) );
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertySerializingConfig<TOwner, short> propertySerializingConfig, CultureInfo cultureInfo)
        {
            return propertySerializingConfig.Using(d => d.ToString(cultureInfo) );
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertySerializingConfig<TOwner, uint> propertySerializingConfig, CultureInfo cultureInfo)
        {
            return propertySerializingConfig.Using(d => d.ToString(cultureInfo) );
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertySerializingConfig<TOwner, ulong> propertySerializingConfig, CultureInfo cultureInfo)
        {
            return propertySerializingConfig.Using(d => d.ToString(cultureInfo) );
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertySerializingConfig<TOwner, ushort> propertySerializingConfig, CultureInfo cultureInfo)
        {
            return propertySerializingConfig.Using(d => d.ToString(cultureInfo) );
        }
        
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertySerializingConfig<TOwner, float> propertySerializingConfig, CultureInfo cultureInfo)
        {
            return propertySerializingConfig.Using(d => d.ToString(cultureInfo) );
        }

        public static PrintingConfig<TOwner> TakeOnly<TOwner>(this PropertySerializingConfig<TOwner, string> config,
            int numberChar)
        {
            return config.Using(str =>
            {
                if (str == null)
                    return "<null>";
                return str.Length < numberChar ? str : str.Substring(0, numberChar);
            });
        }
    }
}