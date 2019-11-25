using System.Collections.Generic;

namespace ObjectPrinting.Tests.TestClasses
{
    public class Student
    {
        public string Name { get; set; }
        public College PlaceOfStudy { get; set; }

        public Student(string name, College placeOfStudy)
        {
            Name = name;
            PlaceOfStudy = placeOfStudy;
        }
    }

    public class College
    {
        public List<Student> StudentList { get; set; }
    }
}