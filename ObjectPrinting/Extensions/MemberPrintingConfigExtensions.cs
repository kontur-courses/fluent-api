using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using ObjectPrinting.Configs;

namespace ObjectPrinting.Extensions
{
    public static class MemberPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TType>
            (this MemberPrintingConfig<TOwner, TType> config, CultureInfo culture)
            where TType : IFormattable
        {
            return (PrintingConfig<TOwner>) config
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(m =>
                    m.GetCustomAttribute(typeof(FormattableTypeAttribute)) != null)
                .Invoke(config, new[] {culture});
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>
            (this MemberPrintingConfig<TOwner, string> config, int length)
        {
            return (PrintingConfig<TOwner>) config
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(m =>
                    m.GetCustomAttribute(typeof(TrimmableTypeAttribute)) != null)
                .Invoke(config, new object[] {length});
        }
    }
}