using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; init; }
        public double Height { get; init; }
        public int Age { get; init; }
        public Status Status { get; set; }
    }

    public enum Status
    {
        Preschooler,
        SchoolStudent,
        Student,
        Worker,
        Retiree
    }
}