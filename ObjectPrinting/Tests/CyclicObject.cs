namespace ObjectPrinting.Tests
{
    class CyclicObject
    {
        public int Id { get; }
        public CyclicObject Other { get; set; }
    }
}
