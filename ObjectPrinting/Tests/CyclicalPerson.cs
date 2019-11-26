namespace ObjectPrinting.Tests
{
    class CyclicalPerson
    {
        public CyclicalPerson NextPerson { get; set; }
        public int Number { get; set; }
    }
}
