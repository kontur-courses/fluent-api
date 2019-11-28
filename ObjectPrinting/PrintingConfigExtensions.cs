using System;
using System.Linq;
using System.Text;

namespace ObjectPrinting
{
    public static class PrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> CutToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> config,
            int length)
        {
            var cut = new Func<string, string>(s => s.Substring(0, Math.Min(s.Length, length)));
            var property = (config as IPropertyPrintingConfig<TOwner, string>).Property;
            var printingConfig = (config as IPropertyPrintingConfig<TOwner, string>).PrintingConfig;
            printingConfig.PropertyToCut.Add(property, cut);
            return  printingConfig;
        }
        
        public static string PrintToString<T>(this T printedObject)
        {
            return ObjectPrinter.For<T>().PrintToString(printedObject);
        }
        
        public static string PrintToString<T>(this T printedObject, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(printedObject);
        }

        public static string GetFullTypeName(this Type type)
        {
            return type.IsGenericType ? GetGenericTypeName(type) : type.Name;
        }

        private static string GetGenericTypeName(this Type type)
        {
            return new StringBuilder()
                .Append(type.GetTypeName())
                .Append("<")
                .Append(type.GetArguments())
                .Append(">")
                .ToString();
        }

        private static string GetTypeName(this Type type)
        {
            var name = type.Name;
            var symbolIndex = name.IndexOf('`');
            if (symbolIndex > 0)
                name = name.Substring(0, symbolIndex);
            return name;
        }

        private static string GetArguments(this Type type)
        {
            var genericArguments = type.GetGenericArguments();
            var arguments = genericArguments.Select(t => t.GetFullTypeName()).ToList();
            return string.Join(", ", arguments);
        }
    }
}