using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting.Tests.Entity
{
    public class NestedClass
    {
        public int Number { get; set; }
        public NestedClass Parent { get; set; }
    }
}
