using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Solved.Tests
{
    class A
    {
        public Guid Id;
        public B bField;
    }

    class B
    {
        public Guid Id;
        public A aField;
    }
}
