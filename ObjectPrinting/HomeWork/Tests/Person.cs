namespace ObjectPrinting.HomeWork.Tests
{
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Person Parent { get; set; }
        public Person Child { get; set; }
        public double Height { get; set; }
        public string Note { get; set; }
        public decimal Salary { get; set; }
        public List<Person> Friends { get; set; }
        public int? LuckyNumber { get; set; }
        public List<List<int>> Matrix { get; set; }
    }
}