using System;
using System.Globalization;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrinting_Should
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;

        [SetUp]
        public void Setup()
        {
            person = new Person 
            {
                Id = new Guid(),
                Name = "Ilya", 
                Age = 20, 
                Height = 181.5, 
            };
        }

        [Test]
        public void PrintToString_WhenExcludedTypesAreSpecified()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<Guid>();
            var expectedString = string.Join(Environment.NewLine, 
                "Person", 
                "\tName = Ilya", 
                "\tHeight = 181,5", 
                "\tAge = 20", 
                "\tGrades = null",
                "\tFriend = null",
                "");

            printer.PrintToString(person).Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_WhenExcludedPropertiesAreSpecified()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Id);

            var expectedString = string.Join(Environment.NewLine,
                "Person",
                "\tName = Ilya",
                "\tHeight = 181,5",
                "\tAge = 20",
                "\tGrades = null",
                "\tFriend = null",
                "");

            printer.PrintToString(person).Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_WhenTheSerializationMethodForTheTypeIsSpecified()
        {
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(i => i.ToString("X"));

            var expectedString = string.Join(Environment.NewLine,
               "Person",
               $"\tId = 00000000-0000-0000-0000-000000000000",
               "\tName = Ilya",
               "\tHeight = 181,5",
               "\tAge = 14",
               "\tGrades = null",
               "\tFriend = null",
               "");

            printer.PrintToString(person).Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_WhenTheSerializationMethodForThePropertyIsSpecified()
        {
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Age).Using(i => i.ToString("X"));

            var expectedString = string.Join(Environment.NewLine,
               "Person",
               $"\tId = 00000000-0000-0000-0000-000000000000",
               "\tName = Ilya",
               "\tHeight = 181,5",
               "\tAge = 14",
               "\tGrades = null",
               "\tFriend = null",
               "");

            printer.PrintToString(person).Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_WhenStringIsClipped()
        {
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).TrimmedToLength(1);

            var expectedString = string.Join(Environment.NewLine,
               "Person",
               $"\tId = 00000000-0000-0000-0000-000000000000",
               "\tName = I",
               "\tHeight = 181,5",
               "\tAge = 20",
               "\tGrades = null",
               "\tFriend = null",
               "");

            printer.PrintToString(person).Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_WhenCultureIsSpecifiedForNumericType()
        {
            var printer = ObjectPrinter.For<Person>().Printing<double>().Using(CultureInfo.InvariantCulture);

            var expectedString = string.Join(Environment.NewLine,
              "Person",
              $"\tId = 00000000-0000-0000-0000-000000000000",
              "\tName = Ilya",
              "\tHeight = 181.5",
              "\tAge = 20",
              "\tGrades = null",
              "\tFriend = null",
              "");

           printer.PrintToString(person).Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_SerializesCyclicReferences()
        {
            var firstPerson = new Person() {  Name = "Ilya" };
            var secondPerson = new Person() { Name = "Kirill", Friend = firstPerson };
            firstPerson.Friend = secondPerson;
            var printer = ObjectPrinter.For<Person>();

            var expectedString = string.Join(Environment.NewLine, 
                "Person", 
                $"\tId = 00000000-0000-0000-0000-000000000000",
                "\tName = Ilya",
                "\tHeight = 0",
                "\tAge = 0",
                "\tGrades = null",
                "\tFriend = Person", 
                $"\t\tId = 00000000-0000-0000-0000-000000000000",
                $"\t\tName = Kirill",
                "\t\tHeight = 0", 
                "\t\tAge = 0",
                "\t\tGrades = null", 
                "");

            printer.PrintToString(firstPerson).Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_SerializesArray()
        {
            var testArray = new[] { 1, 2, 3, 4, 5 };
            var printer = ObjectPrinter.For<int[]>();
            var expectedString = string.Join(Environment.NewLine, "[ 1 2 3 4 5 ]", "");

            printer.PrintToString(testArray).Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_SerializesList()
        {
            var testList = new List<string> { "1", "2", "3", "4", "5" };
            var printer = ObjectPrinter.For<List<string>>();
            var expectedString = string.Join(Environment.NewLine, "[ 1 2 3 4 5 ]", "");

            printer.PrintToString(testList).Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_SerializesDictionary()
        {
            var testDict = new Dictionary<string, double> { { "Первый ключ", 1 }, { "Второй ключ", 2 } };
            var printer = ObjectPrinter.For<Dictionary<string, double>>();

            var expectedString = @"{
	[Первый ключ - 1],
	[Второй ключ - 2],
}
";
            printer.PrintToString(testDict).Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_SerializesArray_WhenInClass()
        {
            person.Grades = new[] { 1.5, 2.4, 3.3, 4.2, 5.1 };
            var printer = ObjectPrinter.For<Person>();
            var expectedString = string.Join(Environment.NewLine,
                "Person",
                $"\tId = 00000000-0000-0000-0000-000000000000",
                "\tName = Ilya",
                "\tHeight = 181,5",
                "\tAge = 20",
                "\tGrades = [ 1,5 2,4 3,3 4,2 5,1 ]",
                "\tFriend = null",
                "");

            printer.PrintToString(person).Should().Be(expectedString);
        }
    }
}