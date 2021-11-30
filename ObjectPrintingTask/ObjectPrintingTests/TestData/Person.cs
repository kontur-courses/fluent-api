using System;
using System.Collections.Generic;

namespace ObjectPrintingTests.TestData
{
    public class Person
    {
        public Dictionary<string, int> dictionary = new Dictionary<string, int>
        {
            { "string", 6 },
            { "Marsell", 7 }
        };

        public Dictionary<int, Dictionary<int, int>> innerDictionary = new Dictionary<int, Dictionary<int, int>>
        {
            {
                0, new Dictionary<int, int>
                {
                    { 0, 0 },
                    { 1, 1 },
                    { 2, 2 }
                }
            },
            {
                1, new Dictionary<int, int>
                {
                    { 1, 0 },
                    { 2, 1 },
                    { 3, 2 }
                }
            }
        };

        public List<List<int>> innerList = new List<List<int>>
        {
            new List<int> { 1, 2, 3 },
            new List<int> { 4, 5, 6 },
            new List<int> { 7, 8, 9 }
        };

        public List<int> list = new List<int> { 1, 2, 3, 4, 5 };

        public Person Parent;

        public double Weight;
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }

        public static Person GetTestInstance()
        {
            return new Person
            {
                Id = Guid.NewGuid(),
                Name = "Marsell",
                Surname = "Radkevich",
                Age = 21,
                Height = 195.5,
                Weight = 83.4
            };
        }
    }
}