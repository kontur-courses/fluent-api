using System;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TParentType> TrimmedToLength<TParentType>(
            this PropertyPrintingConfig<TParentType, string> p, int maxLen)
        {
            Func<object, string> trimmedStr = str => (str as string)?[..Math.Min(((string) str).Length, maxLen)];
            p.Parent.SpecialSerializationForTypes.Add(typeof(string), trimmedStr);
            return p.Parent;
        }
    }
}