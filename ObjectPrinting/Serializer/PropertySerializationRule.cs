using System.Reflection;

namespace ObjectPrinting.Serializer
{
    public delegate bool SerializationFilter(object obj, PropertyInfo excludedProperty);
    public delegate string SerializationFormer(object obj, PropertyInfo currentProperty);

    public struct PropertySerializationRule
    {
        public readonly SerializationFilter FilterHandler;
        public readonly SerializationFormer FormattingHandler;

        public PropertySerializationRule(SerializationFilter filterHandler, SerializationFormer formattingHandler)
        {
            FilterHandler = filterHandler;
            FormattingHandler = formattingHandler;
        }
    }
}