namespace ObjectPrinting.Tests
{
    public class Person
    { 
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
    }

    public class ClassWithCycleReference
    {
        public ClassWithCycleReference CycleReference { get; set; }
    }
}