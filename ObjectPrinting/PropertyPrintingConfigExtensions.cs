using System;
using System.Linq.Expressions;

namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
     public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config) => 
         config(ObjectPrinter.For<T>()).PrintToString(obj);

     public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
         this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
     {
         if (maxLen < 0)
             throw new ArgumentOutOfRangeException(nameof(maxLen), "Max length must be non-negative!");

         IPropertyPrintingConfig<TOwner, string> propertyConfig = propConfig;
         var printingConfig = propertyConfig.ParentConfig; 
         var propertyName = ((MemberExpression)propConfig.MemberSelector.Body).Member.Name;
         
         printingConfig.AddSerializerForProperty(propertyName, value =>
         {
             var val = value as string ?? string.Empty;
             return val.Length > maxLen ? val[..maxLen] : val;
         });
         
         return printingConfig;
     }
}