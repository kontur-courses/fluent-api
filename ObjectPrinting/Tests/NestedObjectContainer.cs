using System.Collections.Generic;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class NestedObjectContainer
    {
        public NestedObjectContainer SubContainer { get; set; }

        public NestedObjectContainer(int nestedness = 0)
        {
            if (nestedness < 20)
                SubContainer = new NestedObjectContainer(nestedness + 1);
        }
    }
}