using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class ParentOfMany : Person
    {
        public IEnumerable<Person> Children { get; set; } = new List<Person>();
    }
}
