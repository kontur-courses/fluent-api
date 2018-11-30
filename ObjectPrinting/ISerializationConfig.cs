using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
