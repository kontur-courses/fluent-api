using System;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propertyPrintingConfig, int length)
        {
            if (propertyPrintingConfig.PropertyInfo == null) 
                return propertyPrintingConfig.ParentConfig;
            
            var propertyName = propertyPrintingConfig.PropertyInfo.Name;
            
            if (propertyPrintingConfig.Serializers.ContainsKey(propertyName))
            {
                var trim = new Func<string, string>(x => x[..length]);
                propertyPrintingConfig.Serializers[propertyName] = Delegate.Combine((Delegate)propertyPrintingConfig.Serializers[propertyName], trim);
            }
            else
                propertyPrintingConfig.Serializers.Add(propertyName, new Func<string, string>(x => x[..length]));
        
            return propertyPrintingConfig.ParentConfig;
        }
    }
}