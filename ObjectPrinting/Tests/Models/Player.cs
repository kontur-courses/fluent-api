using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Player
    {
        public string Name { get; set; }

        public ICollection<int> Scores { get; set; }
    }
}