using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrintingTest.SerializationClasses
{
    public class Hidden
    {
        public int Visible { get; set; } = 1;
        public int WriteOnly { set { } }
        public int PrivateGet { private get; set; }
    }
}
