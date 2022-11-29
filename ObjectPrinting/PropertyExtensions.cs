using System;

namespace ObjectPrinting
{
    public static class PropertyExtensions
    {
        public static IPropertyConfig<TOwner,string> CutString<TOwner>(this IPropertyConfig<TOwner, string> config, int maxLength)
        {
            config.Printer.SetStringCut(config.PropertyExpression, maxLength);
            return config;
        }

        public static IPropertyConfig<TOwner, float> SetCulture<TOwner>(this IPropertyConfig<TOwner, float> config)
        {
            throw new NotImplementedException();
        }
        
        public static IPropertyConfig<TOwner, double> SetCulture<TOwner>(this IPropertyConfig<TOwner, double> config)
        {
            throw new NotImplementedException();
        }
        public static IPropertyConfig<TOwner, DateTime> SetCulture<TOwner>(this IPropertyConfig<TOwner, DateTime> config)
        {
            throw new NotImplementedException();
        }
    }
}