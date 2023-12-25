using System;
using System.Reflection;

namespace ObjectPrinting.PropertyOrField
{
    public interface IPropertyOrField
    {
        string Name { get; }
        Type DataType { get; }
        MemberInfo UnderlyingMember { get; }
        Func<object, object> GetValue { get; }
    }
}