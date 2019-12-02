using System;
using System.Reflection;

namespace ObjectPrinting.Serializer
{
    public struct PropertySerializationRule
    {
        public readonly Func<object, PropertyInfo, bool> FilterHandler;
        public readonly Func<object, PropertyInfo, string> FormattingHandler;

        public PropertySerializationRule(Func<object, PropertyInfo, bool> filterHandler, 
            Func<object, PropertyInfo, string> formattingHandler)
        {
            FilterHandler = filterHandler;
            FormattingHandler = formattingHandler;
        }
    }
}