using System.Collections.Generic;

namespace ObjectPrinting.Tests.TestClasses
{
    public class NameSet
    {
        public string[] Names { get; set; }
    }

    public class RaceResults
    {
        public List<string> Racers { get; set; }
    }

    public class StudentMarks
    {
        public Dictionary<string, int> Marks { get; set; }
    }
}