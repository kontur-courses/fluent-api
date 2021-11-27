using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Serializers
{
    public interface ICollectionSerializer : ISerializer
    {
        public IEnumerable<StringBuilder> SerializeItems(object obj, Nesting nesting);
    }

    public class ArraySerializer : ICollectionSerializer
    {
        private readonly ISerializer objectSerializer;

        public ArraySerializer(ISerializer objectSerializer)
        {
            this.objectSerializer = objectSerializer ?? throw new ArgumentNullException(nameof(objectSerializer));
        }

        public IEnumerable<StringBuilder> SerializeItems(object obj, Nesting nesting)
        {
            ValidateObject(obj);

            var index = 0;
            foreach (var item in (IEnumerable)obj)
            {
                var builder = new StringBuilder();
                builder.Append($"[{index}]: ");
                builder.Append(
                    objectSerializer.Serialize(item, nesting with {Offset = nesting.Offset + builder.Length}));

                yield return builder;
                index++;
            }
        }

        public StringBuilder Serialize(object obj) => Serialize(obj, new Nesting());

        public StringBuilder Serialize(object obj, Nesting nesting)
        {
            var builder = new StringBuilder();
            foreach (var item in SerializeItems(obj, nesting))
                builder.Append(item);

            return builder;
        }

        public bool CanSerialize(object obj)
        {
            var type = obj.GetType();
            return type.IsArray
                   || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        private void ValidateObject(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (!CanSerialize(obj))
                throw new ArgumentException($"Can't serialize {nameof(obj)}", nameof(obj));
        }
    }
}