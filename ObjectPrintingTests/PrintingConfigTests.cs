using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class PrintingConfigTests
    {
        private PrintingConfig<Person> printer;
        private Person person;

        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
            person = new Person
            {
                Id = Guid.NewGuid(),
                Name = "mattgroy",
                Age = 18,
                Height = 1.89
            };
//            const string expected = "Person\r\n\tId = Guid\r\n\tName = mattgroy\r\n\tHeight = 1,89\r\n\tAge = 18\r\n";
        }

        [Test]
        public void ExcludeTypeFromSerialization()
        {
            const string expected = "Person\r\n\tName = mattgroy\r\n\tHeight = 1,89\r\n\tAge = 18\r\n";
            printer.Excluding<Guid>();
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ExcludePropertyFromSerialization()
        {
            const string expected = "Person\r\n\tId = Guid\r\n\tName = mattgroy\r\n\tAge = 18\r\n";
            printer.Excluding(p => p.Height);
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ChangeCultureForNumeric_Double()
        {
            const string expected = "Person\r\n\tId = Guid\r\n\tName = mattgroy\r\n\tHeight = 1.89\r\n\tAge = 18\r\n";
            printer.Printing<double>().Using(CultureInfo.InvariantCulture);
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ChangeSerializationForProperty_Age()
        {
            const string expected = "Person\r\n\tId = Guid\r\n\tName = mattgroy\r\n\tHeight = 1,89\r\n\tAge = 21\r\n";
            printer.Printing(p => p.Age).Using(i => (i + 3).ToString());
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ChangeSerializationForType_String()
        {
            const string expected = "Person\r\n\tId = Guid\r\n\tName = c#\r\n\tHeight = 1,89\r\n\tAge = 18\r\n";
            printer.Printing<string>().Using(s => "c#");
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void TrimmedToLength_LengthLessThanPropertyLength()
        {
            const string expected = "Person\r\n\tId = Guid\r\n\tName = matt\r\n\tHeight = 1,89\r\n\tAge = 18\r\n";
            printer.Printing(p => p.Name).TrimmedToLength(4);
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void TrimmedToLength_LengthBiggerThanPropertyLength()
        {
            const string expected = "Person\r\n\tId = Guid\r\n\tName = mattgroy\r\n\tHeight = 1,89\r\n\tAge = 18\r\n";
            printer.Printing(p => p.Name).TrimmedToLength(30);
            printer.PrintToString(person).Should().Be(expected);
        }
    }
}