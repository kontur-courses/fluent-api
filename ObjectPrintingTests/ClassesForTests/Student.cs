namespace ObjectPrintingTests.ClassesForTests
{
    public class Student
    {
        public string Name { get; }

        public string University;

        public Student(string name, string university)
        {
            Name = name;
            University = university;
        }
    }
}