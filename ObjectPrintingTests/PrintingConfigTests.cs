using System;
using System.Collections.Generic;
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

        [Test]
        public void ChangeSerializationForProperty_Name_ThenCall_TrimmedToLength()
        {
            const string expected = "Person\r\n\tId = Guid\r\n\tName = Hello\r\n\tHeight = 1,89\r\n\tAge = 18\r\n";
            printer
                .Printing(p => p.Name).Using(s => "Hello World")
                .Printing(p => p.Name).TrimmedToLength(5);
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void TrimmedToLength_ThenCall_ChangeSerializationForProperty_Name()
        {
            const string expected = "Person\r\n\tId = Guid\r\n\tName = Hello\r\n\tHeight = 1,89\r\n\tAge = 18\r\n";
            printer
                .Printing(p => p.Name).TrimmedToLength(5)
                .Printing(p => p.Name).Using(s => "Hello World");
            printer.PrintToString(person).Should().Be(expected);
        }

        public class MyClass
        {
            public List<Person> MyList { get; set; }
            public Dictionary<int, Person> MyDict { get; set; }
        }

        [Test]
        public void PrintList()
        {
            const string expected = 
                "MyClass\r\n" +
                "\tMyList = List`1\r\n\t{\r\n" +
                "\t\tPerson\r\n\t\t\tId = Guid\r\n\t\t\tName = mattgroy\r\n\t\t\tHeight = 1,89\r\n\t\t\tAge = 18\r\n" +
                "\t\tPerson\r\n\t\t\tId = Guid\r\n\t\t\tName = Vsauce\r\n\t\t\tHeight = 1,46\r\n\t\t\tAge = 9\r\n\t}\r\n" +
                "\tMyDict = null\r\n";
            var myClassInstance = new MyClass()
            {
                MyList = new List<Person> {person, new Person {Name = "Vsauce", Age = 9, Height = 1.46}}
            };
            var printerResult = myClassInstance.PrintToString();
            printerResult.Should().Be(expected);
        }

        [Test]
        public void PrintDictionary()
        {
            const string expected = 
                "MyClass\r\n" +
                "\tMyList = null\r\n" +
                "\tMyDict = Dictionary`2\r\n\t{\r\n" +
                "\t\tKeyValuePair`2\r\n\t\t\tKey = 1\r\n\t\t\tValue = Person\r\n" +
                "\t\t\t\tId = Guid\r\n\t\t\t\tName = mattgroy\r\n\t\t\t\tHeight = 1,89\r\n\t\t\t\tAge = 18\r\n\r\n" +
                "\t\tKeyValuePair`2\r\n\t\t\tKey = 2\r\n\t\t\tValue = Person\r\n" +
                "\t\t\t\tId = Guid\r\n\t\t\t\tName = Vsauce\r\n\t\t\t\tHeight = 1,46\r\n\t\t\t\tAge = 9\r\n\r\n\t}\r\n";
            var myClassInstance = new MyClass()
            {
                MyDict = new Dictionary<int, Person>
                {
                    {1, person},
                    {2, new Person {Name = "Vsauce", Age = 9, Height = 1.46}}
                }
            };
            var printerResult = myClassInstance.PrintToString();
            printerResult.Should().Be(expected);
        }
    }
}