using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting.Configuration
{
    public interface IPropertyConfigurator
    {
        IReadOnlyList<SerializationTarget> Targets { get; }
    }

    public interface IPropertyConfigurator<TOwner, TProperty> : IPropertyConfigurator
    {
        IPropertySerializer<TProperty> AppliedSerializer { get; }
        PrintingConfig<TOwner> Using(IPropertySerializer<TProperty> serializer);
    }

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