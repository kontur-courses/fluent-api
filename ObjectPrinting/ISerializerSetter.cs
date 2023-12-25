using System;
using System.Reflection;

namespace ObjectPrinting
{
    public interface ISerializerSetter
    {
        public void SetSerializer<T>(Func<T, string> serializer, PropertyInfo propertyInfo);
    }
}