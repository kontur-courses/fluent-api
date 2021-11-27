namespace ObjectPrintingTests.Persons
{
    public class NestedPerson : Person
    {
        public Person Child { get; set; }
    }
}