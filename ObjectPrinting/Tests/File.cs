using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class File
    {
        public string field;
        public string Name { get; set; }

        public Dictionary<string, string> Attributes { get; set; }

        public List<string> SimilarNames { get; set; }

        public string[] Copies { get; set; }
    }
}