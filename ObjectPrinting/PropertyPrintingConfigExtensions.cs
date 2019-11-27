using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }
        
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {   
            var  parentConfig = ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
            if (((IPropertyPrintingConfig<TOwner, string>) propConfig).MemberSelector.Body is MemberExpression memberExpression)
                ((IPrintingConfig) parentConfig).PropertyStringsLength[memberExpression.Member.Name] = maxLen;
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }

    }
}