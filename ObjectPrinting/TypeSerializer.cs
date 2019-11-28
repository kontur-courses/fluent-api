using System;

namespace ObjectPrinting
{
    public class TypeSerializer
    {
        public readonly Type Type;
        private Func<object, string> serializer;

        private TypeSerializer(Type type, Func<object, string> serializer)
        {
            this.Type = type;
            this.serializer = serializer;
        }

        public static TypeSerializer Create<T>(Func<T, string> serializer)
        {
            return new TypeSerializer(typeof(T), o => serializer((T) o));
        }

        public string Serialize(object objectToSerialize)
        {
            if (Type.IsInstanceOfType(objectToSerialize))
                return serializer(objectToSerialize);
            throw new ArgumentException();
        }
    }
}