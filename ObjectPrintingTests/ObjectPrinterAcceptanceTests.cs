using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 180.5, SubPerson = new SubPerson() };
            person.SubPerson.Age = 15;
            person.SubPerson.Person = person;

            var printer = ObjectPrinter.For<Person>();

            var actualString = printer.Exclude<double>()
                .Printing(p => p.Age)
                .Using(age => (age + 1000).ToString())
                .And.Printing<double>()
                .Using(CultureInfo.InvariantCulture)
                .And.Printing(p => p.Name)
                .Trim(1)
                .And.Exclude(p => p.Name)
                .WithMaxRecursion(2)
                .PrintToString(person);

            actualString.Should().Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tAge = 1019\r\n\tSubPerson = SubPerson\r\n\t\tPerson = Maximum recursion has been reached\r\n\t\tAge = 15\r\n\tPublicField = null\r\n");
        }
    }
}