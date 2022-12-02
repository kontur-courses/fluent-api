using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class LibraryRecords
    {
        public readonly string[] BooksCollection =
        {
            "A",
            "B",
            "C",
            "D",
            "E"
        };

        public List<string> ActiveUsers { get; set; } = new List<string>();
        public Dictionary<string, DateTime> ReadersVisits { get; set; } = new Dictionary<string, DateTime>();
    }
}