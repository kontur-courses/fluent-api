using System;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        [Test]
        public void ObjectPrinter_ExcludingCertainTypeOfProperty_Int()
        {
            var person = new Person {Name = "Vasa", Id = new Guid(), Age = 19, Height = 170.0};

            var printer = ObjectPrinter.For<Person>().Excluding<int>();
            var actual = printer.PrintToString(person);

            actual.Should().Be("Person\r\n\tId = Guid\r\n\tName = Vasa\r\n\tHeight = 170\r\n");
        }
        
        [Test]
        public void ObjectPrinter_ExcludingCertainTypeOfProperty_String()
        {
            var person = new Person {Name = "Vasa", Id = new Guid(), Age = 19, Height = 170.0};

            var printer = ObjectPrinter.For<Person>().Excluding<string>();
            var actual = printer.PrintToString(person);

            actual.Should().Be("Person\r\n\tId = Guid\r\n\tHeight = 170\r\n\tAge = 19\r\n");
        }
        
        [Test]
        public void ObjectPrinter_ExcludingCertainTypeOfProperty_SevetalTypes()
        {
            var person = new Person {Name = "Vasa", Id = new Guid(), Age = 19, Height = 170.0};

            var printer = ObjectPrinter.For<Person>().Excluding<string>()
                .Excluding<int>()
                .Excluding<Guid>()
                .Excluding<double>();
            var actual = printer.PrintToString(person);

            actual.Should().Be("Person\r\n");
        }
    }
}