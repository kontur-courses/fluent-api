using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Tests
{
    internal class House
    {
        public string Address { get; set; }
        public House LeftAdjacentBuilding { get; set; }
        public House RightAdjacentBuilding { get; set; }

    }
}
