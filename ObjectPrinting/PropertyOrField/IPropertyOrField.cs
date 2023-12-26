using System;
using System.Reflection;

namespace ObjectPrinting.PropertyOrField
{
    internal interface IPropertyOrField
    {
        string Name { get; }
        Type DataType { get; }
        MemberInfo UnderlyingMember { get; }
        Func<object, object> GetValue { get; }
    }
}