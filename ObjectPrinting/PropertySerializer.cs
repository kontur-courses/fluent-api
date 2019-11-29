using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertySerializer
    {
        public readonly PropertyInfo Property;
        private Func<object, string> serializer;

        private PropertySerializer(PropertyInfo property, Func<object, string> serializer)
        {
            this.Property = property;
            this.serializer = serializer;
        }

        public static PropertySerializer Create<T>(PropertyInfo property, Func<T, string> serializer)
        {
            return new PropertySerializer(property, o => serializer((T) o));
        }

        public string Serialize(object objectToSerialize)
        {
            if (Property.PropertyType.IsInstanceOfType(objectToSerialize))
                return serializer(objectToSerialize);
            throw new ArgumentException($"{objectToSerialize} is not instance of {Property.PropertyType}");
        }
    }
}