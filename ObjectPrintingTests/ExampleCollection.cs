using System.Collections.Generic;

namespace ObjectPrintingTests
{
    public class ExampleCollections<T>
    {
        public T[] Collection { get; set; }
        public List<T> ListCollection { get; set; }
        public Dictionary<T, T> DictionaryCollection { get; set; }
    }
}