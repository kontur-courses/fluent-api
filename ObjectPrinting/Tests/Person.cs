using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Person Parent { get; set; }
        public string[] FavoriteMovies { get; set; }
        public List<Person> BestFriends { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }

    }
}