using System;
using System.Linq;
using System.Reflection;
using ObjectPrinter.Interfaces;

namespace ObjectPrinter.ObjectPrinter
{
    public static class Extensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return Printer.For<T>().PrintToString(obj);
        }
        
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(Printer.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this MemberConfig<TOwner, string> propConfig, int maxLen)
        {
            var parentConfig = ((IMemberConfig<TOwner>)propConfig).ParentConfig;
            parentConfig.CustomTypeSerializer.Add(typeof(string), obj => string.Join("", (obj as string ?? "").Take(maxLen)));
            return parentConfig;
        }
        
        public static PrintingConfig<TOwner> SetMaxLengthForType<TOwner, TPropType>(this MemberConfig<TOwner, TPropType> propConfig, int maxLen)
        {
            var parentConfig = ((IMemberConfig<TOwner>)propConfig).ParentConfig;
            parentConfig.CustomTypeSerializer.Add(typeof(TPropType), obj => (string.Join("", (obj.ToString() ?? "").Take(maxLen))+"\n"));
            return parentConfig;
        }
    }
}