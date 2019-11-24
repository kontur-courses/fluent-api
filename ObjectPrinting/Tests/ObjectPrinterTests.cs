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
    [TestFixture]
    class ObjectPrinterTests
    {
        [Test]
        public void ObjectPrinter_PrintingWithoutChanges()
        {
            var testingPerson = new Person();
            var printer = ObjectPrinter.For<Person>();

            var outString = printer.PrintToString(testingPerson);
            outString.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n	Age = 0\r\n");
        }

        [Test]
        public void ObjectPrinter_PrintingExceptType()
        {
            var testingPerson = new Person();
            var printer = ObjectPrinter.For<Person>().Excluding<int>();

            var outString = printer.PrintToString(testingPerson);
            outString.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n");
        }

        [Test]
        public void ObjectPrinter_PrintingWithCustomSerializer()
        {
            var testingPerson = new Person();
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(a=>"int");

            var outString = printer.PrintToString(testingPerson);
            outString.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n	Age = int\r\n");
        }

        [TestCase(4.2, "de-DE", "Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 4,2\r\n	Age = 0\r\n")]
        [TestCase(4.2, "en-GB", "Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 4.2\r\n	Age = 0\r\n")]
        public void ObjectPrinter_PrintingDoubleWithCustomCulture(double height, string culture, string expected)
        {
            var testingPerson = new Person();
            testingPerson.Height = height;
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(new CultureInfo(culture));

            var outString = printer.PrintToString(testingPerson);
            outString.Should().Be(expected);
        }


        [Test]
        public void ObjectPrinter_PrintingWithExcludedAge()
        {
            var testingPerson = new Person();
            var printer = ObjectPrinter.For<Person>()
                .Excluding(x => x.Age);

            var outString = printer.PrintToString(testingPerson);
            outString.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n");
        }
    }
}
