using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertySerializerCollection
    {
        public readonly Dictionary<PropertyInfo, PropertySerializer> PropertySerializators;

        public PropertySerializerCollection()
        {
            PropertySerializators = new Dictionary<PropertyInfo, PropertySerializer>();
        }

        public PropertySerializerCollection(PropertySerializerCollection collection)
        {
            PropertySerializators =
                new Dictionary<PropertyInfo, PropertySerializer>(collection.PropertySerializators);
        }

        public void Add(PropertySerializer serializer)
        {
            PropertySerializators.Add(serializer.property, serializer);
        }

        public bool ContainsSerializerFor(PropertyInfo info)
        {
            return PropertySerializators.ContainsKey(info);
        }
        
        public PropertySerializer GetSerializerFor(PropertyInfo info)
        {
            return PropertySerializators[info];
        }
    }
}