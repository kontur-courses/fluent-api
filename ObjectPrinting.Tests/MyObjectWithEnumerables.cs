using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting.Tests
{
    class MyObjectWithEnumerables
    {
        public int[] Array { get; set; }

        public IList<Person> ListOfComplexObjects { get; set; }

        public IDictionary<string, string> Dictionary { get; set; }
    }
}
