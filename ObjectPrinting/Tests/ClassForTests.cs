using System.Runtime.InteropServices;

namespace ObjectPrinting.Tests
{
    public class ClassForTests
    {
        public double Height = 0;
        public ClassForTests Friend { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public int Age { get; set; }


        public ClassForTests(string firstName = "", string surname = "", int age = 0, ClassForTests cc = null, double height = 0)
        {
            Friend = cc;
            FirstName = firstName;
            Surname = surname;
            Age = age;
            Height = height;
        }
    }
}