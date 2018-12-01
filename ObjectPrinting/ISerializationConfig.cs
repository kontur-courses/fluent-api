using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    interface ISerializationConfig<TOwner>
    {
        void SetTypeSerialization<T>(Func<T, string> serializer);
        void SetPropertySerialization(MemberInfo memberExp, Func<TOwner, string> serializer);
        void SetCulture<T>(CultureInfo culture);
        void SetTrim(MemberInfo memberExp, int length);
    }
}
