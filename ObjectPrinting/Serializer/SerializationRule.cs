using System;
using System.Reflection;

namespace ObjectPrinting
{
    public delegate bool SerializationFilter(object obj, PropertyInfo excludedProperty);
    public delegate string SerializationFormer(object obj, PropertyInfo currentProperty, string indent, int nestingLevel);

    public struct SerializationRule
    {
        public readonly SerializationFilter FilterHandler;
        public readonly SerializationFormer ResultHandler;

        public SerializationRule(SerializationFilter filterHandler, SerializationFormer resultHandler)
        {
            this.FilterHandler = filterHandler;
            this.ResultHandler = resultHandler;
        }
    }
}