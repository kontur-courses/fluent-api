namespace ObjectPrinting.Tests
{
    public class Name
    {
        public string FirstName { get; set; }
        public string SecondName { get; set; }
    }

    public class PersonWithComplexName
    {
        public Name PersonName { get; set; }
        public int Age { get; set; }
    }
}