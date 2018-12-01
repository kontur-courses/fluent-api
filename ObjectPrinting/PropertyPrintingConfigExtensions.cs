using System;
using System.Text.RegularExpressions;

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
            var settingsHolder =
                (IPrintingConfigurationHolder) ((IPrintingConfig<TOwner, string>) propConfig).ParentConfig;
            var memberSelector = (propConfig as IMemberSelector<TOwner, string>).memberSelector;
            var matchCollection = new Regex(string.Format("{0}.(\\w*)", memberSelector.Parameters[0].Name));
            var trimedProperty = matchCollection.Match(memberSelector.Body.ToString()).Value
                .Replace(memberSelector.Parameters[0].Name + ".", "");
            settingsHolder.TrimedParams.Add(trimedProperty, maxLen);
            return ((IPrintingConfig<TOwner, string>) propConfig).ParentConfig;
        }
    }
}