using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting.Tests
{
    class CyclicalPerson
    {
        public CyclicalPerson Next { get; set; }
        public int Number { get; set; }
    }
}
