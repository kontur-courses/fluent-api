using System;

namespace ObjectPrinting.Methods
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner>
            TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, uint maxLen)
        {
            var maxLenInt = (int)Math.Min(maxLen, (uint)int.MaxValue);

            return propConfig.Using(s =>
            {
                Range startIndex = ..Math.Min(s.Length, maxLenInt);
                return s?[startIndex];
            });


        }

    }
}