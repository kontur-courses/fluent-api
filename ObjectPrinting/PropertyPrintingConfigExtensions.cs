using System;

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
            var parent = ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
            Func<object, string> trimmedStr = str => (str as string)?[..Math.Min(((string) str).Length, maxLen)];
            parent.SpecialSerializations.Add(new Tuple<Type, string>(typeof(string), null), trimmedStr);
            return parent;
        }
    }
}