using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            var  parentConfig = ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
            if (((IPropertyPrintingConfig<TOwner, string>) propConfig).MemberSelector.Body is MemberExpression memberExpression)
                ((IPrintingConfig) parentConfig).PropertyStringsLength[memberExpression.Member] = maxLen;
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }
        
        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(this PropertyPrintingConfig<TOwner, TPropType> propConfig, CultureInfo culture) where TPropType: IFormattable
        {
            var  parentConfig = ((IPropertyPrintingConfig<TOwner, TPropType>)propConfig).ParentConfig;
            ((IPrintingConfig) parentConfig).TypeCultures[typeof(TPropType)] = culture;
            return parentConfig;
        }
    }
}