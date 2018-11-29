using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ObjectPrinting
{
    [TestFixture]
    public class PrintingOverwriteTests
    {
        private Student testStudent;

        [SetUp]
        public void SetUp()
        {
            testStudent = new Student() {Group = "MEH-272201", Name = "Name"};
        }
    

    [Test]
        public void Printing_ShouldNotOverwritePrintingMethods_ThenInSameTimeConfigurateTypePrintingAndPropertyPrinting()
        {
            var resString = testStudent.PrintToString(config => config
                .Printing<string>().Using(s => s.ToUpper())
                .Printing(s => s.Group).Using(s => s.ToLower()));
            resString.Should().Be($"Student{Environment.NewLine}" +
                                  $"\tName = NAME{Environment.NewLine}" +
                                  $"\tGroup = meh-272201");
        }
    }

    public class Student
    {
        public string Name { get; set; }
        public string Group { get; set; }
    }
}
