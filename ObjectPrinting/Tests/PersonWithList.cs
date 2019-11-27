using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting.Tests
{
    class PersonWithList
    {
        public int Number { get; set; }
        public List<PersonWithList> PeopleWithList { get; set; }
    }
}
