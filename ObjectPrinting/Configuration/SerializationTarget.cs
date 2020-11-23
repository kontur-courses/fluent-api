using System;
using System.Reflection;

namespace ObjectPrinting.Configuration
{
    /// <summary>
    /// Property or field to be serialized
    /// </summary>
    public class SerializationTarget
    {
        private readonly Func<object, object> valueGetter;

        public SerializationTarget(PropertyInfo propertyInfo)
        {
            valueGetter = propertyInfo.GetValue;
        }

        public SerializationTarget(FieldInfo fieldInfo)
        {
            valueGetter = fieldInfo.GetValue;
        }

        public Type MemberType { get; set; }
        public Type OwnerType { get; set; }

        public object GetValue(object owner)
        {
            if (owner.GetType() != OwnerType)
                throw new ArgumentException($"Owner must have type {OwnerType}, but was {owner.GetType()}");
            return valueGetter.Invoke(owner);
        }
    }
}