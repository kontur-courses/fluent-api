using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtension
    {        
        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner, TPropType> config,
            CultureInfo cultureInfo)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, TPropType>) config)
                .ParentConfig;
            parentConfig.typeCultureInfo[typeof(TPropType)] = cultureInfo;
            return parentConfig;
        }
        
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> config,
            int length)
        {
            var propertySelector = ((IPropertyPrintingConfig<TOwner, string>) config)
                .PropertySelector;
            var propertyInfo = (PropertyInfo)((MemberExpression) propertySelector.Body).Member;

            var parentConfig = ((IPropertyPrintingConfig<TOwner, string>) config).ParentConfig;
            parentConfig.trimmedProperties[propertyInfo] = length;
            
            return parentConfig;
        }
    }
}