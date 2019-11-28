using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class TypeSerializerCollection
    {
        public readonly Dictionary<Type, TypeSerializer> TypeSerializators;

        public TypeSerializerCollection()
        {
            TypeSerializators = new Dictionary<Type, TypeSerializer>();
        }

        public TypeSerializerCollection(TypeSerializerCollection collection)
        {
            TypeSerializators =
                new Dictionary<Type, TypeSerializer>(collection.TypeSerializators);
        }

        public void Add(TypeSerializer serializer)
        {
            TypeSerializators.Add(serializer.Type, serializer);
        }

        public bool ContainsSerializerFor(Type info)
        {
            return TypeSerializators.ContainsKey(info);
        }
        
        public TypeSerializer GetSerializerFor(Type type)
        {
            return TypeSerializators[type];
        }
    }
}