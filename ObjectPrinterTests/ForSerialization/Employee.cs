namespace ObjectPrinterTests.ForSerialization
{
    public class Employee
    {
        public string Name { get; set; }
        public Employee Chief;
        public Employee Subordinate;
    }
}