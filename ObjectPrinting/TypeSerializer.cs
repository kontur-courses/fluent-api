using System;

namespace ObjectPrinting
{
    public class TypeSerializer
    {
        private Type type;
        private Func<object, string> serializer;

        private TypeSerializer(Type type, Func<object, string> serializer)
        {
            this.type = type;
            this.serializer = serializer;
        }

        public static TypeSerializer CreateSerializer<T>(Type type, Func<T, string> serializer)
        {
            return new TypeSerializer(type, o => serializer((T) o));
        }

        public string Serialize(object objectToSerialize)
        {
            if (type.IsInstanceOfType(objectToSerialize))
                return serializer(objectToSerialize);
            throw new ArgumentException();
        }
    }
}