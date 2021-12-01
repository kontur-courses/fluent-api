using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    internal interface IPrintingConfig<TOwner>
    {
        void Exclude(MemberInfo member);
        void WithSerializer<TProperty>(MemberInfo member, Func<TProperty, string> serializer);
        void WithTrimLength(int length);
        void Exclude<T>();
        void WithSerializer<T>(Func<T, string> serializer);
        void WithCulture<TProperty>(MemberInfo member, CultureInfo cultureInfo) where TProperty : IFormattable;
        void WithTrimLength(MemberInfo member, int length);
        void WithCulture<TProperty>(CultureInfo cultureInfo) where TProperty : IFormattable;
    }
}