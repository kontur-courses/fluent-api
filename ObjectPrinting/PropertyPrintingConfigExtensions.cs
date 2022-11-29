using System;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propertyPrintingConfig, int length)
        {
            if (propertyPrintingConfig.MemberInfo == null) 
                return propertyPrintingConfig.ParentConfig;
            
            var propertyName = propertyPrintingConfig.MemberInfo.Name;
            var trim = new Func<string, string>(x => x.Length >= length ? x[..length] : x);
            
            if (propertyPrintingConfig.Serializers.ContainsKey(propertyName))
                propertyPrintingConfig.Serializers[propertyName] = Delegate.Combine((Delegate)propertyPrintingConfig.Serializers[propertyName], trim);
            else
                propertyPrintingConfig.Serializers.Add(propertyName, trim);
        
            return propertyPrintingConfig.ParentConfig;
        }
    }
}