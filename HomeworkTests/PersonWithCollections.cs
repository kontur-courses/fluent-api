using System;
using System.Collections.Generic;

namespace HomeworkTests
{
    public class PersonWithCollections
    {
        public List<int> List { get; set; }
        public int Number { get; set; } = 12378;
        public Dictionary<string, float> Dict { get; set; }
        public Queue<Guid> Guids { get; set; }
    }
}