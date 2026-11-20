using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Tests.Models
{
    public class RecursivePerson
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public RecursivePerson Children { get; set; }
    }
}
