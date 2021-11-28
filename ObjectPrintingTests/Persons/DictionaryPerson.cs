using System.Collections.Generic;

namespace ObjectPrintingTests.Persons
{
    public class DictionaryPerson
    {
        public Dictionary<NestedPerson, CollectionPerson> Persons { get; set; }
    }
}