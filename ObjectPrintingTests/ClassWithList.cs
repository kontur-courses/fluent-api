using System.Collections.Generic;

namespace ObjectPrintingTests
{
    public class ClassWithList
    {
        public List<double> Values { get; set; }
        public double Value { get; set; }

        public ClassWithList()
        {
            Values = new List<double>();
            Value = 0;
        }
    }
}
