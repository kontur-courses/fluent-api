using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Serializers
{
    public class DictionarySerializer : KeyValueSerializer<IDictionary>
    {
        public DictionarySerializer(ISerializer objectSerializer) : base(objectSerializer) { }

        protected override string GetHeader(IDictionary dictionary) =>
            $"Dictionary[{dictionary.Count}]";

        protected override IEnumerable<KeyValuePair<object, object>> GetKeyValues(IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
                yield return new KeyValuePair<object, object>(entry.Key, entry.Value);
        }

        protected override (StringBuilder Value, Nesting Nesting) SerializeIndexer(object key, Nesting nesting)
        {
            var indexer = objectSerializer.Serialize(key, nesting with {Level = nesting.Level + 1});
            return IsMultiline(indexer)
                ? GetMultilineMarker(indexer, nesting)
                : ListSerializer.SerializeArrayIndexer(indexer, nesting);
        }

        private static bool IsMultiline(StringBuilder indexer)
        {
            for (var i = 0; i < indexer.Length; i++)
                if (indexer[i] == '\n')
                    return true;

            return false;
        }

        private static (StringBuilder Value, Nesting Nesting) GetMultilineMarker(StringBuilder indexer, Nesting nesting)
        {
            var n = Environment.NewLine;
            var nextNesting = nesting with {Level = nesting.Level + 1};

            var indexMarker = new StringBuilder("[" + $"{n}{nextNesting.Indentation}");
            indexMarker.Append(indexer);
            indexMarker.Append($"{n}{nesting.Indentation}" + "]: " + $"{n}{nextNesting.Indentation}");

            return (indexMarker, nextNesting);
        }
    }
}