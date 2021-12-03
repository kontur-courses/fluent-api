using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Tests
{
    public class ClassWithDict
    {
        public Dictionary<string, string> Dict { get; set; }

        public ClassWithDict()
        {
            Dict = new Dictionary<string, string>();
        }
    }
}
