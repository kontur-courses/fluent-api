using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }

        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        internal static IEnumerable<ElementInfo> GetElements(this object obj)
        {
            const BindingFlags flag = BindingFlags.Public | BindingFlags.Instance;
            var type = obj.GetType();
            foreach (var field in type.GetFields(flag))
                yield return new ElementInfo(field);
            foreach (var property in type.GetProperties(flag))
                yield return new ElementInfo(property);
        }
    }
}