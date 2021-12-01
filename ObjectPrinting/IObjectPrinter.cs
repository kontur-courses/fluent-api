using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public interface IObjectPrinter
    {
        string PrintToString(object obj);
        void SetTrimLength(int trimLength);
        void SetTrimLength(MemberInfo member, int trimLength);
        void SetCulture(Type type, CultureInfo cultureInfo);
        void SetCulture(MemberInfo member, CultureInfo cultureInfo);
        void SetSerializer(Type type, Func<object, string> p);
        void SetSerializer(MemberInfo member, Func<object, string> p);
        void Exclude(Type type);
        void Exclude(MemberInfo member);
    }
}