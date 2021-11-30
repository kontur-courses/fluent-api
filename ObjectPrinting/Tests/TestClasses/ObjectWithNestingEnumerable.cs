using System.Collections;
using System.Collections.Generic;

namespace ObjectPrinting.Tests.TestClasses
{
    public class ObjectWithNestingEnumerable
    {
        public IEnumerable<IEnumerable> Collection { get; set; }
    }
}