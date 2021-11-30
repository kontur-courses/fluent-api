using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Serializers
{
    public abstract class KeyValueSerializer<T> : ISerializer
    {
        protected readonly ISerializer objectSerializer;

        protected KeyValueSerializer(ISerializer objectSerializer)
        {
            this.objectSerializer = objectSerializer ?? throw new ArgumentNullException(nameof(objectSerializer));
        }

        public bool CanSerialize(object obj) => TryCast(obj, out _);

        public StringBuilder Serialize(object obj, Nesting nesting)
        {
            if (!TryCast(obj, out var castedObject))
                throw new ArgumentException($"Can't serialize {obj.GetType()}", nameof(obj));

            return new StringBuilder().AppendJoin(Environment.NewLine, SerializeItems(castedObject, nesting));
        }

        private static bool TryCast(object obj, out T casted)
        {
            switch (obj)
            {
                case null:
                    throw new ArgumentNullException(nameof(obj));
                case T tObj:
                    casted = tObj;
                    return true;
                default:
                    casted = default;
                    return false;
            }
        }

        private IEnumerable<StringBuilder> SerializeItems(T obj, Nesting nesting)
        {
            yield return new StringBuilder(GetHeader(obj));

            foreach (var (key, value) in GetKeyValues(obj))
            {
                var builder = new StringBuilder(nesting.Indentation);
                var (stringBuilder, nextNesting) = SerializeIndexer(key, nesting);
                builder.Append(stringBuilder);
                builder.Append(objectSerializer.Serialize(value, nextNesting));
                yield return builder;
            }
        }

        protected abstract string GetHeader(T obj);
        protected abstract IEnumerable<KeyValuePair<object, object>> GetKeyValues(T obj);
        protected abstract (StringBuilder Value, Nesting Nesting) SerializeIndexer(object key, Nesting nesting);
    }
}