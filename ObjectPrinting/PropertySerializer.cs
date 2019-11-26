using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertySerializer
    {
        private PropertyInfo property;
        private Func<object, string> serializer;

        public PropertySerializer(PropertyInfo property, Func<object, string> serializer)
        {
            this.property = property;
            this.serializer = serializer;
        }
        public static PropertySerializer CreateSerializer<T>(PropertyInfo property, Func<T, string> serializer)
        {
            return new PropertySerializer(property, o => serializer((T) o));
        }

        public string Serialize(object objectToSerialize)
        {
            if (property.PropertyType.IsInstanceOfType(objectToSerialize))
                return serializer(objectToSerialize);
            throw new ArgumentException();
        }
    }
}