namespace ObjectPrinting.Tests
{
    public class CyclicPerson
    {
        public string Name { get; set; }
        public CyclicPerson Child { get; set; }
    }
}
