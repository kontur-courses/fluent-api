using System.Reflection;

namespace ObjectPrinting.Serializer
{
    public delegate bool SerializationFilter(object obj, PropertyInfo excludedProperty);
    public delegate string SerializationFormer(object obj, PropertyInfo currentProperty);

    public struct PropertySerializationRule
    {
        public readonly SerializationFilter FilterHandler;
        public readonly SerializationFormer ResultHandler;

        public PropertySerializationRule(SerializationFilter filterHandler, SerializationFormer resultHandler)
        {
            this.FilterHandler = filterHandler;
            this.ResultHandler = resultHandler;
        }
    }
}