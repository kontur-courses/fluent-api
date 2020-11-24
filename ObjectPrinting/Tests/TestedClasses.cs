using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public int Weight;
        public List<Person> Children { get; }
        public Dictionary<string, int> CustomDict { get; }
        public Person Parent { get; set; }

        public Person()
        {
            Children = new List<Person>();
            CustomDict = new Dictionary<string, int>();
        }
    }

    public class Foo
    {
        public string Name { get; set; }
        public Foo Next { get; set; }
        public Foo Friend { get; set; }
    }

    public class Bar
    {
        public Foo Foo { get; set; }
        public string Name { get; set; }
    }

    class Foo1
    {
        public string Name { get; set; }
        public Foo1 Next { get; set; }

        protected bool Equals(Foo1 other) => Name == other.Name;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Foo1) obj);
        }

        public override int GetHashCode() => (Name != null ? Name.GetHashCode() : 0);
    }
}