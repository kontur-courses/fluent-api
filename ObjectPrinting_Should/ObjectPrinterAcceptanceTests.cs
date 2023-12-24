using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrinting_Should
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person = null!;

        [SetUp]
        public void Setup()
        {
            person = new Person { Name = "Alex", Age = 19, Height = 179.5, Id = new Guid() };
        }

        [Test]
        public void PrintToString_SkipsExcludedTypes()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<Guid>();
            var expectedString = string.Join(Environment.NewLine, "Person", "\tName = Alex", "\tHeight = 179,5", "\tAge = 19", "");
            var outputString = printer.PrintToString(person);
            outputString.Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_SkipsExcludedProperty()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Id);

            var expectedString = string.Join(Environment.NewLine, "Person", "\tName = Alex", "\tHeight = 179,5", "\tAge = 19", "");
            var outputString = printer.PrintToString(person);
            outputString.Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_UsesCustomSerializator_WhenGivenToType()
        {
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(i => i.ToString("X"));

            var expectedString = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = Alex", "\tHeight = 179,5", "\tAge = 13", "");
            var outputString = printer.PrintToString(person);
            outputString.Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_UsesCustomSerialization_WhenGivenToProperty()
        {
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Age).Using(i => i.ToString("X"));

            var expectedString = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = Alex", "\tHeight = 179,5", "\tAge = 13", "");
            var outputString = printer.PrintToString(person);
            outputString.Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_TrimsStringProperties_WhenTrimmingIsSet()
        {
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).TrimmedToLength(1);

            var expectedString = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = A", "\tHeight = 179,5", "\tAge = 19", "");
            var outputString = printer.PrintToString(person);
            outputString.Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_UsesCustomCulture_WhenGivenToNumericType()
        {
            var printer = ObjectPrinter.For<Person>().Printing<double>().Using(CultureInfo.InvariantCulture);

            var expectedString = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = Alex", "\tHeight = 179.5", "\tAge = 19", "");
            var outputString = printer.PrintToString(person);
            outputString.Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_SerializesClass_WhenCalledFromItInstance()
        {
            var outputString = person.PrintToString();
            var expectedString = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = Alex", "\tHeight = 179,5", "\tAge = 19", "");
            outputString.Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_SerializesClass_WhenCalledFromItInstanceWithConfig()
        {
            var outputString = person.PrintToString(s => s.Excluding(p => p.Age));
            var expectedString = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = Alex", "\tHeight = 179,5", "");
            outputString.Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_SerializesCyclicReferences()
        {
            var firstPerson = new Person() { Age = 20, Name = "Ben" };
            var secondPerson = new Person() { Age = 20, Name = "John", Sibling = firstPerson};
            firstPerson.Sibling = secondPerson;

            var outputString = firstPerson.PrintToString();
            var expectedString = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = Alex", "\tHeight = 179,5", "");
            outputString.Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_SerializesArray()
        {
            var numbers = new[] { 1.1, 2.2, 3.3, 4.4, 5.5 };
            var outputString = numbers.PrintToString();
            var expectedString = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = Alex", "\tHeight = 179,5", "");
            outputString.Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_SerializesList()
        {
            var numbers = new List<double> { 1.1, 2.2, 3.3, 4.4, 5.5 };
            var outputString = numbers.PrintToString();
            var expectedString = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = Alex", "\tHeight = 179,5", "");
            outputString.Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_SerializesDictionary()
        {
            var numbers = new Dictionary<string, double> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
            var outputString = numbers.PrintToString();
            var expectedString = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = Alex", "\tHeight = 179,5", "");
            outputString.Should().Be(expectedString);
        }
    }
}