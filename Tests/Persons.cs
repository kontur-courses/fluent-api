using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class Person1
    {
        public Guid Id { get; set; }

        public Person2 Parent { get; set; }
    }

    public class Person2
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
