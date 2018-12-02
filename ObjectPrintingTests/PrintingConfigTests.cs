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
        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
            person = new Person
            {
                Id = Guid.Empty,
                Name = "mattgroy",
                Age = 18,
                Height = 1.89
            };
//            const string expected = "Person\r\n\tId = Guid\r\n\tName = mattgroy\r\n\tHeight = 1,89\r\n\tAge = 18";
        }

        private readonly string emptyGuidString = Guid.Empty.ToString();
        private PrintingConfig<Person> printer;
        private Person person;

        public class MyClass
        {
            public int[] MyArray { get; set; }
            public List<Person> MyList { get; set; }
            public Dictionary<int, Person> MyDict { get; set; }
        }

        public class Foo
        {
            public Bar MyBar { get; set; }
        }

        public class Bar
        {
            public Foo MyFoo { get; set; }
        }

        [Test]
        public void ChangeCultureForNumeric_Double()
        {
            var culture = CultureInfo.InvariantCulture;
            var expected = $"Person\r\n\tId = {emptyGuidString}\r\n\tName = mattgroy\r\n\tHeight = 1.89\r\n\tAge = 18";
            printer.Printing<double>().Using(culture);
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ChangeSerializationForProperty_Age()
        {
            var expected = $"Person\r\n\tId = {emptyGuidString}\r\n\tName = mattgroy\r\n\tHeight = {person.Height.ToString(CultureInfo.CurrentCulture)}\r\n\tAge = 21";
            printer.Printing(p => p.Age).Using(i => (i + 3).ToString());
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ChangeSerializationForProperty_Name_ThenCall_TrimmedToLength()
        {
            var expected = $"Person\r\n\tId = {emptyGuidString}\r\n\tName = Hello\r\n\tHeight = {person.Height.ToString(CultureInfo.CurrentCulture)}\r\n\tAge = 18";
            printer
                .Printing(p => p.Name).Using(s => "Hello World")
                .Printing(p => p.Name).TrimmedToLength(5);
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ChangeSerializationForType_String()
        {
            var expected = $"Person\r\n\tId = {emptyGuidString}\r\n\tName = c#\r\n\tHeight = {person.Height.ToString(CultureInfo.CurrentCulture)}\r\n\tAge = 18";
            printer.Printing<string>().Using(s => "c#");
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ExcludePropertyFromSerialization()
        {
            var expected = $"Person\r\n\tId = {emptyGuidString}\r\n\tName = mattgroy\r\n\tAge = 18";
            printer.Excluding(p => p.Height);
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ExcludeTypeFromSerialization()
        {
            var expected = $"Person\r\n\tName = mattgroy\r\n\tHeight = {person.Height.ToString(CultureInfo.CurrentCulture)}\r\n\tAge = 18";
            printer.Excluding<Guid>();
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void PrintDictionary()
        {
            var myClassInstance = new MyClass
            {
                MyDict = new Dictionary<int, Person>
                {
                    {1, person},
                    {2, new Person {Name = "Vsauce", Age = 9, Height = 1.46}}
                }
            };

            var expected =
                "MyClass\r\n" +
                "\tMyArray = null\r\n" +
                "\tMyList = null\r\n" +
                "\tMyDict = Dictionary`2\r\n\t{\r\n" +
                "\t\tKeyValuePair`2\r\n\t\t\tKey = 1\r\n\t\t\tValue = Person\r\n" +
                $"\t\t\t\tId = {emptyGuidString}\r\n\t\t\t\tName = mattgroy\r\n\t\t\t\tHeight = {person.Height.ToString(CultureInfo.CurrentCulture)}\r\n\t\t\t\tAge = 18\r\n" +
                "\t\tKeyValuePair`2\r\n\t\t\tKey = 2\r\n\t\t\tValue = Person\r\n" +
                $"\t\t\t\tId = {emptyGuidString}\r\n\t\t\t\tName = Vsauce\r\n\t\t\t\tHeight = {1.46.ToString(CultureInfo.CurrentCulture)}\r\n\t\t\t\tAge = 9\r\n\t}}";
            var printerResult = myClassInstance.PrintToString();
            printerResult.Should().Be(expected);
        }

        [Test]
        public void PrintList()
        {
            var myClassInstance = new MyClass
            {
                MyList = new List<Person> { person, new Person { Name = "Vsauce", Age = 9, Height = 1.46 } }
            };

            var expected =
                "MyClass\r\n" +
                "\tMyArray = null\r\n" +
                "\tMyList = List`1\r\n\t{\r\n" +
                $"\t\tPerson\r\n\t\t\tId = {emptyGuidString}\r\n\t\t\tName = mattgroy\r\n\t\t\tHeight = {person.Height.ToString(CultureInfo.CurrentCulture)}\r\n\t\t\tAge = 18\r\n" +
                $"\t\tPerson\r\n\t\t\tId = {emptyGuidString}\r\n\t\t\tName = Vsauce\r\n\t\t\tHeight = {1.46.ToString(CultureInfo.CurrentCulture)}\r\n\t\t\tAge = 9\r\n\t}}\r\n" +
                "\tMyDict = null";
            var printerResult = myClassInstance.PrintToString();
            printerResult.Should().Be(expected);
        }

        [Test]
        public void TrimmedToLength_LengthBiggerThanPropertyLength()
        {
            var expected = $"Person\r\n\tId = {emptyGuidString}\r\n\tName = mattgroy\r\n\tHeight = {person.Height.ToString(CultureInfo.CurrentCulture)}\r\n\tAge = 18";
            printer.Printing(p => p.Name).TrimmedToLength(30);
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void TrimmedToLength_LengthLessThanPropertyLength()
        {
            var expected = $"Person\r\n\tId = {emptyGuidString}\r\n\tName = matt\r\n\tHeight = {person.Height.ToString(CultureInfo.CurrentCulture)}\r\n\tAge = 18";
            printer.Printing(p => p.Name).TrimmedToLength(4);
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void TrimmedToLength_ThenCall_ChangeSerializationForProperty_Name()
        {
            var expected = $"Person\r\n\tId = {emptyGuidString}\r\n\tName = Hello\r\n\tHeight = {person.Height.ToString(CultureInfo.CurrentCulture)}\r\n\tAge = 18";
            printer
                .Printing(p => p.Name).TrimmedToLength(5)
                .Printing(p => p.Name).Using(s => "Hello World");
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void PrintLongCollection_NotAllElements()
        {
            const string expected =
                "MyClass\r\n\tMyArray = Int32[]\r\n\t{\r\n" +
                "\t\t0\r\n\t\t1\r\n\t\t2\r\n\t\t3\r\n\t\t4\r\n\t\t...\r\n\t}\r\n" +
                "\tMyList = null\r\n\tMyDict = null";

            var array = new int[10];
            for (var i = 0; i < 10; i++)
                array[i] = i;

            var myClassInstance = new MyClass { MyArray = array };
            myClassInstance.PrintToString(config => config.SetCollectionLookDepthTo(5)).Should().Be(expected);
        }

        [Test]
        public void Print_WhenRecursion()
        {
            const string expected = 
                "Foo\r\n\tMyBar = Bar\r\n\t\tMyFoo = ...";

            var myFoo = new Foo();
            myFoo.MyBar = new Bar {MyFoo = myFoo};

            myFoo.PrintToString(config => config.SetPropetiesLookDepthTo(1)).Should().Be(expected);
        }
    }
}