using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Player<T>
    {
        public string Name { get; set; }

        public ICollection<T> Scores { get; set; }
    }
}