using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Serializers
{
    public abstract class CollectionSerializer<T> : ISerializer
    {
        protected readonly ISerializer objectSerializer;

        protected CollectionSerializer(ISerializer objectSerializer)
        {
            this.objectSerializer = objectSerializer ?? throw new ArgumentNullException(nameof(objectSerializer));
        }

        public bool CanSerialize(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return obj is T;
        }

        public StringBuilder Serialize(object obj) => Serialize(obj, new Nesting());

        public StringBuilder Serialize(object obj, Nesting nesting)
        {
            ValidateObject(obj);
            return new StringBuilder().AppendJoin(Environment.NewLine, SerializeItems(obj, nesting));
        }

        private void ValidateObject(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (!CanSerialize(obj))
                throw new ArgumentException($"Can't serialize {obj.GetType()}", nameof(obj));
        }

        private IEnumerable<StringBuilder> SerializeItems(object obj, Nesting nesting)
        {
            yield return new StringBuilder(GetHeader((T)obj));

            foreach (var (key, value) in GetKeyValues((T)obj))
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