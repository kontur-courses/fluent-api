using System;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Cut<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig,
            int maxLen)
        {
            if (propConfig.Parent.MaxStringLength.ContainsKey(propConfig.MemberInfo))
                throw new InvalidOperationException("The string length for this property/field has already been set before");
            propConfig.Parent.MaxStringLength[propConfig.MemberInfo] = maxLen;
            return propConfig.Parent;
        }

        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }
    }
}