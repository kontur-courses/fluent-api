using System.Collections;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class FooBar
    {
        public string Name { get; set; }
        public int Value;
        public ICollection<int> Numbers;
        public FooBar Parent;
    }
}