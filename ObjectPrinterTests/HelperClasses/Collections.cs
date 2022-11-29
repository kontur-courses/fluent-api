using System.Collections.Generic;

namespace ObjectPrinterTests.HelperClasses
{
    public class Collections
    {
        public Collections()
        {
            Array = new[] { 1, 2, 3, 4, 5, 7, 8, 9 };
            List = new List<int>(Array) { 10 };
            Dictionary = new Dictionary<string, int>
            {
                ["one"] = 1,
                ["two"] = 2,
                ["three"] = 3,
                ["four"] = 4,
                ["five"] = 5,
                ["six"] = 6
            };
        }

        public int[] Array { get; set; }

        public List<int> List { get; set; }

        public  Dictionary<string, int> Dictionary { get; set; }

    }
}