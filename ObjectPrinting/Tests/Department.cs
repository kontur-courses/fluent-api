namespace ObjectPrinting.Tests
{
    public class Department
    {
        public string Name { get; set; }
        public Company ParentCompany { get; set; }
        public Department SubDepartment { get; set; }
    }
}