using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class TypeSerializerCollection
    {
        private readonly Dictionary<Type, TypeSerializer> typeSerializators;

        public TypeSerializerCollection()
        {
            typeSerializators = new Dictionary<Type, TypeSerializer>();
        }

        public TypeSerializerCollection(TypeSerializerCollection collection)
        {
            typeSerializators =
                new Dictionary<Type, TypeSerializer>(collection.typeSerializators);
        }

        public void Add(TypeSerializer serializer)
        {
            typeSerializators.Add(serializer.Type, serializer);
        }

        public bool ContainsSerializerFor(Type info)
        {
            return typeSerializators.ContainsKey(info);
        }
        
        public TypeSerializer GetSerializerFor(Type type)
        {
            return typeSerializators[type];
        }
    }
}