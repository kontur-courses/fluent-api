using System.Collections;
using System.Collections.Generic;

namespace ObjectPrinting.Tests.TestClasses
{
    public class ObjectWithNestedEnumerable
    {
        public IEnumerable<IEnumerable> Collection { get; set; }
    }
}