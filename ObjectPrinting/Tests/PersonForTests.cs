using System;

namespace ObjectPrinting.Tests
{
    class PersonForTests
    {
        public PersonForTests Parent;
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age;

        private Guid id;
    }
}
