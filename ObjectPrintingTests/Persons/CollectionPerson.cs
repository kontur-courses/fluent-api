using System.Collections.Generic;

namespace ObjectPrintingTests.Persons
{
    public class CollectionPerson
    {
        public List<Person> ChildrenList { get; set; }
        public Person[] ChildrenArray { get; set; }
        public Dictionary<int, Person> ChildrenDictionary { get; set; }
    }
}