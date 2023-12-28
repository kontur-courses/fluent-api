using System;
using System.Reflection;

namespace ObjectPrinting.Interfaces
{
    public interface ISerializerSetter
    {
        public void SetSerializer<T>(Func<T, string> serializer, PropertyInfo propertyInfo);
    }
}