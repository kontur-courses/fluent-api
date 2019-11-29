using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertySerializerCollection
    {
        private readonly Dictionary<PropertyInfo, PropertySerializer> propertySerializators;

        public PropertySerializerCollection()
        {
            propertySerializators = new Dictionary<PropertyInfo, PropertySerializer>();
        }

        public PropertySerializerCollection(PropertySerializerCollection collection)
        {
            propertySerializators =
                new Dictionary<PropertyInfo, PropertySerializer>(collection.propertySerializators);
        }

        public void Add(PropertySerializer serializer)
        {
            propertySerializators.Add(serializer.Property, serializer);
        }

        public bool ContainsSerializerFor(PropertyInfo info)
        {
            return propertySerializators.ContainsKey(info);
        }
        
        public PropertySerializer GetSerializerFor(PropertyInfo info)
        {
            return propertySerializators[info];
        }
    }
}