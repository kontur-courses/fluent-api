namespace ObjectPrinting.Tests
{
    public class ClassForTests
    {
        public ClassForTests Friend { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public int Age { get; set; }


        public ClassForTests(string name = "", string surName = "", int age = 0, ClassForTests cc = null)
        {
            Friend = cc;
            Name = name;
            SurName = surName;
            Age = age;
        }
    }
}