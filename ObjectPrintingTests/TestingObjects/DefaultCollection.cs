using System.Collections.Generic;

namespace ObjectPrintingTests
{
    public class DefaultCollection
    {
        public readonly List<int> list = new List<int>
        {
            102, 15, 47
        };

        public int[] array => list.ToArray();

        public Dictionary<int, string> dict = new Dictionary<int, string>
        {
            {8, "eight"},
            {15, "fifteen"}
        };
    }
}