using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    internal interface IPrintingConfig<out TOwner>
    {
        void SetCultureFor<T>(CultureInfo cultureInfo);

        void SetTrimmingFor(MemberInfo memberInfo, int maxLength);

        void SetSerializerFor(MemberInfo memberInfo, Func<TOwner, string> serializer);

        void SetSerializerFor<T>(Func<T, string> serializer);
    }
}
