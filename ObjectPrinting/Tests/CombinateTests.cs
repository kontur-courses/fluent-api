using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class Student
    {
        public string Name { get; set; }
        public string Group { get; set; }
    }

    [TestFixture]
    public class CombinateTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person("Name", 22.5,23);
        }

        [Test]
        public void Printing_ShouldNotOverwritePrintingMethods_ThenInSameTimeConfigurateTypePrintingAndPropertyPrinting()
        {
            var testStudent = new Student() { Group = "MEH-272201", Name = "Name" };
            var resString = testStudent.PrintToString(config => config
                .Printing<string>().Using(s => s.ToUpper())
                .Printing(s => s.Group).Using(s => s.ToLower()));
            resString.Should().Be($"Student{Environment.NewLine}" +
                                  $"\tName = NAME{Environment.NewLine}" +
                                  $"\tGroup = meh-272201");
        }
    

    [Test]
        public void CombinateTest1()
        {
            person.Child = new Person("Child", 3.4, 2);
            var serialize = person.PrintToString(cnf => cnf
                .Printing(p => p.Child).Using(c => c.Name)
                .Printing<int>().ChangeCultureInfo(CultureInfo.InstalledUICulture)
                .Printing(p => p.Name).Using(s => s.ToLower()));
            serialize.Should()
                .Be($"Person{Environment.NewLine}" +
                    $"\tName = name{Environment.NewLine}" +
                    $"\tHeight = 22,5{Environment.NewLine}" +
                    $"\tAge = 23{Environment.NewLine}" +
                    $"\tPet = null{Environment.NewLine}" +
                    $"\tChild = Child");
        }
    }
}
