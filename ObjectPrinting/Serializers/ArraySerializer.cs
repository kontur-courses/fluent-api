using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Serializers
{
    public class ArraySerializer : CollectionSerializer<IList>
    {
        public ArraySerializer(ISerializer objectSerializer) : base(objectSerializer) { }

        public static (StringBuilder Value, Nesting Nesting) SerializeArrayIndexer(object key, Nesting nesting)
        {
            var indexMarker = new StringBuilder("[");
            indexMarker.Append(key);
            indexMarker.Append("]: ");
            return (indexMarker, nesting with {Offset = nesting.Offset + indexMarker.Length});
        }

        protected override (StringBuilder Value, Nesting Nesting) SerializeIndexer(object key, Nesting nesting) =>
            SerializeArrayIndexer(key, nesting);

        protected override string GetHeader(IList list)
        {
            var type = list.GetType();
            var count = list.Count;
            return type.IsArray ? $"Array[{count}]" : $"List[{count}]";
        }

        protected override IEnumerable<KeyValuePair<object, object>> GetKeyValues(IList list)
        {
            for (var i = 0; i < list.Count; i++)
                yield return new KeyValuePair<object, object>(i, list[i]);
        }
    }
}