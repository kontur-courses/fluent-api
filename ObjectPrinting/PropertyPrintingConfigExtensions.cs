using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            if(maxLen <= 0)
                throw new ArgumentException("maxLen must be positive");
            var printingConfig = ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
            var member = ((IPropertyPrintingConfig<TOwner, string>) propConfig).Member;
            if (member is null)
                ((IPrintingConfig<TOwner>) printingConfig).Trim = x => x.Length > maxLen ? x.Substring(0, maxLen) : x;
            else
            {
                var trimmer = ((IPrintingConfig<TOwner>) printingConfig).Trimmer;
                var propInfo =
                    ((MemberExpression) member.Body).Member as PropertyInfo;
                trimmer[propInfo.Name] = x => x.Length > maxLen ? x.Substring(0, maxLen) : x;
            }

            return printingConfig;
        }
    }
}