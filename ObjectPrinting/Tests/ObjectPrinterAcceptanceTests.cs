using NUnit.Framework;
using FluentAssertions;
using System;
using System.Globalization;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        Person person = new Person();

        [SetUp]
        public void Setup()
        {
            person = new Person { Name = "Alex", Age = 19, Height = 12.34, Id = new Guid() };
        }

        [Test]
        public void PrintToString_SkipExcludedPropertys()
        {
            var printer = ObjectPrinter.For<Person>().Exclude(x=>x.Age);
            var serialized = printer.PrintToString(person);
            serialized.Should().Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tHeight = 12,34\r\n\tparent = null\r\n");
        }

        [Test]
        public void PrintToString_SkipExcludedTypes()
        {
            var printer = ObjectPrinter.For<Person>().Exclude<int>();
            var serialized = printer.PrintToString(person);
            serialized.Should().Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tHeight = 12,34\r\n\tparent = null\r\n");
        }

        [Test]
        public void PrintToString_UseCustomSerializerForCertainType()
        {
            var printer = ObjectPrinter.For<Person>().Serialize<int>().Using(x=> "Type Serializer");
            var serialized = printer.PrintToString(person);
            serialized.Should().Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tHeight = 12,34\r\nType Serializer\tparent = null\r\n");
        }

        [Test]
        public void PrintToString_UseCustomSerializerForCertainProperty()
        {
            var printer = ObjectPrinter.For<Person>().Serialize(x=>x.Name).Using(x=>"Property Serializer");
            var serialized = printer.PrintToString(person);
            serialized.Should().Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Property Serializer\r\n\tHeight = 12,34\r\n\tAge = 19\r\n\tparent = null\r\n");
        }

        [Test]
        public void PrintToString_UseCustomCultureForCertainProperty()
        {
            var printer = ObjectPrinter.For<Person>().Serialize<double>().Using<Double>(CultureInfo.InvariantCulture);
            var serialized = printer.PrintToString(person);
            serialized.Should().Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tHeight = 12,34\r\n\tAge = 19\r\n\tparent = null\r\n");
        }

        [Test]
        public void PrintToString_TrimLimitStringLength()
        {
            var printer = ObjectPrinter.For<Person>().Serialize(x=>x.Name).Trim(1);
            var serialized = printer.PrintToString(person);
            serialized.Should().Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = A\r\n\tHeight = 12,34\r\n\tAge = 19\r\n\tparent = null\r\n");
        }


        [Test]
        public void PrintToString_ParsinCyclingLinks()
        {
            var parent = new Person();
            parent.Id = new Guid("00000000-0000-0000-0000-012147483648");
            person.parent = parent;
            parent.parent = person;
            var printer = ObjectPrinter.For<Person>();
            var serialized = printer.PrintToString(person);
            serialized.Should().Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tHeight = 12,34\r\n\tAge = 19\r\n\tparent = Person\r\n\t\tId = 00000000-0000-0000-0000-012147483648\r\n\t\tName = null\r\n\t\tHeight = 0\r\n\t\tAge = 0\r\n\t\tparent = Person\r\n");
        }

        [Test]
        public void PrintToString_ParsinArrays()
        {
            var arr = new IEnums();
            arr.Array = new int[] { 1, 2, 3 };

            var printer = ObjectPrinter.For<IEnums>();
            var serialized = printer.PrintToString(arr);
            serialized.Should().Be("IEnums\r\n\tArray = { 1, 2, 3 }\r\n\t\r\n\t\r\n");
        }

        [Test]
        public void PrintToString_ParsinLists()
        {
            var list = new IEnums();
            list.List = new List<int>() {1,2,3 };

            var printer = ObjectPrinter.For<IEnums>();
            var serialized = printer.PrintToString(list);
            serialized.Should().Be("IEnums\r\n\t\r\n\tList = { 1, 2, 3 }\r\n\t\r\n");
        }

        [Test]
        public void PrintToString_ParsinDicts()
        {
            var dict = new IEnums();
            dict.Dict = new Dictionary<char, int>() { {'a',1 },{'b',2 },{'c',3 } };

            var printer = ObjectPrinter.For<IEnums>();
            var serialized = printer.PrintToString(dict);
            serialized.Should().Be("IEnums\r\n\t\r\n\t\r\n\tDict = { (a, 1), (b, 2), (c, 3) }\r\n");
        }
    }
}