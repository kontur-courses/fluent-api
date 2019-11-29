using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Person BestFriend;
        public string NickName;
        public double Width;

        public Person(string name, int age, double height, int[] classMarks)
        {
            Name = name;
            Age = age;
            Height = height;
            ClassMarks = classMarks;
        }

        public Person()
        {
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public int[] ClassMarks { get; set; }
    }
}