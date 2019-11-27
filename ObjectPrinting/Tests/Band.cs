using System.Collections.Generic;

namespace ObjectPrinting
{
    public class Band
    {
        public string Name { get; set; }
        public List<string> MembersNames { get; set; }
        public Dictionary<string, string> Instruments { get; set; }
    }
}
