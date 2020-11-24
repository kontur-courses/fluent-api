using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class House
    {
        public House()
        {
            ApartmentOwner = new Dictionary<int, string>();
        }

        public Dictionary<int, string> ApartmentOwner { get; }
        public int[] Apartments { get; set; }
    }
}