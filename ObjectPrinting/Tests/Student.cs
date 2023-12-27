namespace ObjectPrinting.Tests
{
    public class Student : Person
    {
        public Person Teacher { get; set; }

        public Person Friend { get; set; }
    }
}