using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Tests.DTOs;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinterTests
    {
        [Test]
        public void Printer_UseExcludeTypeIntFromPerson_ShouldExcludeAgeFromSerialization()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .ExcludeType<int>();
            var result = printer.PrintToString(person);

            result.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n");
        }

        [Test]
        public void Printer_UseExcludeTypeIntFromInt_ShouldReturnNewLine()
        {
            var printer = ObjectPrinter.For<int>()
                .ExcludeType<int>();
            var result = printer.PrintToString(11);

            result.Should().Be("\r\n");
        }

        [Test]
        public void Printer_UseSerializeType_ShouldSerializeSelectedTypeInPersonAsSpecified()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .SerializeType<string>(x => x.ToUpper());
            var result = printer.PrintToString(person);

            result.Should().Be("Person\r\n\tId = Guid\r\n\tName = ALEX\r\n\tHeight = 0\r\n\tAge = 19\r\n");
        }

        [Test]
        public void Printer_UseSerializeType_ShouldSerializeSelectedType()
        {
            var printer = ObjectPrinter.For<string>()
                .SerializeType<string>(x => x.ToUpper());
            var result = printer.PrintToString("Test");

            result.Should().Be("TEST\r\n");
        }

        [Test]
        public void Printer_SetCulture_ShouldSerializeSelectedTypeWithDot()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 175.5 };

            var printer = ObjectPrinter.For<Person>()
                .SetCulture<double>(CultureInfo.InvariantCulture);
            var result = printer.PrintToString(person);

            result.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 175.5\r\n\tAge = 19\r\n");
        }

        [Test]
        public void Printer_SetCultureInPrimitiveType_ShouldSerializeItWithDot()
        {
            var printer = ObjectPrinter.For<double>()
                .SetCulture<double>(CultureInfo.InvariantCulture);
            var result = printer.PrintToString(175.5);

            result.Should().Be("175.5\r\n");
        }

        [Test]
        public void Printer_SetCultureInPrimitiveTypeOnDifferentType_ShouldSerializeItWithoutDot()
        {
            var printer = ObjectPrinter.For<double>()
                .SetCulture<int>(CultureInfo.InvariantCulture);
            var result = printer.PrintToString(175.5);

            result.Should().Be("175,5\r\n");
        }

        [Test]
        public void Printer_UseConfigurePropertyOnStringType_ShouldAllowToCropIt()
        {
            var person = new Person { Name = "Alexxx", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .ConfigureProperty(x => x.Name).Crop(5);
            var result = printer.PrintToString(person);

            result.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alexx\r\n\tHeight = 0\r\n\tAge = 19\r\n");
        }

        [Test]
        public void Printer_UseConfigureProperty_ShouldSetSerialization()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .ConfigureProperty(x => x.Age).SetSerialization(x => x + "000");
            var result = printer.PrintToString(person);

            result.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19000\r\n");
        }

        [Test]
        public void Printer_UseExcludeProperty_ShouldExcludeitFromSerialization()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .ExcludeProperty(x => x.Height);
            var result = printer.PrintToString(person);

            result.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tAge = 19\r\n");
        }

        [Test]
        public void Printer_PrintToString_ShouldPreventRecursion()
        {
            var person1 = new RecursivePerson { Name = "Alex", Age = 19 };
            var person2 = new RecursivePerson { Name = "Bob", Age = 22 };
            person1.Father = person2;
            person2.Father = person1;

            var printer = ObjectPrinter.For<RecursivePerson>();
            var result = printer.PrintToString(person1);

            result.Should().Be(
                "RecursivePerson\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19\r\n\tFather = " +
                "RecursivePerson\r\n\t\tId = Guid\r\n\t\tName = Bob\r\n\t\tHeight = 0\r\n\t\tAge = 22\r\n\t\tFather = RecursivePerson (On nesting level: 0)\r\n");
        }

        [Test]
        public void Printer_PrintListToString_ShouldEPrintList()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var list = new List<Person>
            {
                person,
                person,
                person
            };

            var printer = ObjectPrinter.For<Person>()
                .ExcludeProperty(x => x.Height);
            var result = printer.PrintListToString(list);

            result.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tAge = 19\r\n" +
                               "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tAge = 19\r\n" +
                               "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tAge = 19\r\n");
        }
    }
}