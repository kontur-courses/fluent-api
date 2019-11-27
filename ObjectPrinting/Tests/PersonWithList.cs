using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    class PersonWithList
    {
        public int Number { get; set; }
        public List<PersonWithList> PeopleWithList { get; set; }
    }
}
