namespace ObjectPrinting.Tests.TestClasses
{
    public class PersonWithTwoParents
    {
        public string Name { get; set; }
        public PersonWithTwoParents FirstParent { get; set; }
        public PersonWithTwoParents SecondParent { get; set; }
    }
}