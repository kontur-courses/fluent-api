using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void ObjectPrinter_ExcludingIntType_ObjectWithoutIntProperties()
        {
            var person = new Person { Name = "Alex", Age = 19 };
    
            var printer = ObjectPrinter.For<Person>();
            var s1 = printer.Excluding<int>().PrintToString(person);
            const string expected = "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tHeight = 0\r\n";
            s1.Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_ExcludingHeight_ObjectWithoutHeight()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>();
            const string expected = "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tAge = 19\r\n";
            var s1 = printer.Excluding(x => x.Height).PrintToString(person);
            s1.Should().Be(expected);
        }

        [Test] public void ObjectPrinter_PrintingPropertyUsingSomeConditional_ObjectWithModifiedProperty()
        {
            var person = new Person { Name = "Alex", Age = 15 };
            var printer = ObjectPrinter.For<Person>();
            const string expected = "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = F\r\n";
            var s1 = printer.Printing(x => x.Age).Using(x=> x.ToString("X")).PrintToString(person);
            s1.Should().Be(expected);
        }
        [Test]
        public void ObjectPrinter_ChangingPropertiesByType_ObjectWithModifiedProperty()
        {
            var person = new Person { Name = "Alex", Age = 15, Height = 1.2};
            var printer = ObjectPrinter.For<Person>();
            const string expected = "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tHeight = 2,4\r\n\tAge = 15\r\n";
            var s1 = printer.Printing<double>().Using(x=> (x * 2).ToString()).PrintToString(person);
            s1.Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_ChangingPropertiesUsingCulture_ObjectWithModifiedProperty()
        {
            var person = new Person { Name = "Alex", Age = 15, Height = 2.4 };
            var printer = ObjectPrinter.For<Person>();
            const string expected = "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tHeight = 2.4\r\n\tAge = 15\r\n";
            var s1 = printer.Printing<double>().Using(CultureInfo.InvariantCulture).PrintToString(person);
            s1.Should().Be(expected);
        }
        [Test]
        public void ObjectPrinter_TrimmedStringProperties_ObjectWithModifiedProperty()
        {
            var person = new Person { Name = "Alex", Age = 15, Height = 2.4, Id = Guid.Empty};
            var printer = ObjectPrinter.For<Person>();
            const string expected = "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tHeight = 2,4\r\n\tAge = 15\r\n";
            var s1 = printer.Printing<string>().TrimmedToLength(10).PrintToString(person);
            s1.Should().Be(expected);
        }
        [Test]
        public void ObjectPrinter_TrimmedStringPropertiesButCroppingLengthLess0_ObjectWithModifiedProperty()
        {
            var person = new Person { Name = "Alex", Age = 15, Height = 2.4, Id = Guid.Empty};
            var printer = ObjectPrinter.For<Person>();
            Action action = () =>
            {
                printer.Printing<string>().TrimmedToLength(-10).PrintToString(person);
            };
            action.Should().Throw<ArgumentException>("Error: The length of the truncated string cannot be negative");
        }
        [Test]
        public void ObjectPrinter_PropertyRefersItself_Object()
        {
            var kid = new Kid { Name = "Pasha"};
            var parent = new Kid{Name = "Lev"};
            kid.Parent = parent;
            parent.Parent = kid;
            
            
            
            var printer = ObjectPrinter.For<Kid>();
            const string expected = "Kid\r\n\tName = Pasha\r\n\tParent = Kid\r\n\t\tName = Lev\r\n\t\tParent = (Cycle) ObjectPrinting.Tests.Kid\r\n";
            var s1 = printer.PrintToString(kid);
            s1.Should().Be(expected);
        }
        [Test]
        public void ObjectPrinter_PrintingDictionaryProperty_Object()
        {
            var collections = new Collections();
            collections.Dictionary = new Dictionary<int, string>()
            {
                {1, "hello"},
                {2, "hell"},
                {3, "hel"},
                {4, "he"},
                {5, "h"},
            };
            var printer = ObjectPrinter.For<Collections>();
            const string expected = "Collections\r\n\tDictionary = \r\n\t\t" +
                                    "1\r\n : hello\r\n\t\t" +
                                    "2\r\n : hell\r\n\t\t" +
                                    "3\r\n : hel\r\n\t\t" +
                                    "4\r\n : he\r\n\t\t" +
                                    "5\r\n : h\r\n\t" +
                                    "Array = \r\nnull\r\n\tList = \r\nnull\r\n\tPersons = \r\nnull\r\n";
            var s1 = printer.PrintToString(collections);
            s1.Should().Be(expected);
        }
        [Test]
        public void ObjectPrinter_PrintObjectWithListProperty_Object()
        {
            var collections = new Collections();
            collections.List = new List<int>() { 1, 2, 3 };
            var printer = ObjectPrinter.For<Collections>();
            const string expected = "Collections\r\n\tDictionary = \r\nnull\r\n\tArray = \r\nnull\r\n\tList = \r\n\t\t1\r\n\t\t2\r\n\t\t3\r\n\t" +
                                    "Persons = \r\nnull\r\n";
            var s1 = printer.PrintToString(collections);
            s1.Should().Be(expected);
        }
        [Test]
        public void ObjectPrinter_InArrayGenericObjects_Object()
        {
            var collections = new Collections();
            collections.List = new List<int>() { 1, 2, 3 };
            collections.Array = new [] { collections.List };
            var printer = ObjectPrinter.For<Collections>();
            const string expected = "Collections\r\n\tDictionary = \r\nnull\r\n\tArray = " +
                                    "\r\n\t\tList`1" +
                                    "\r\n\t\t1" +
                                    "\r\n\t\t2" +
                                    "\r\n\t\t3" +
                                    "\r\n\tList = " +
                                    "\r\n\t\t1" +
                                    "\r\n\t\t2" +
                                    "\r\n\t\t" +
                                    "3\r\n\t" +
                                    "Persons = \r\nnull\r\n";
            var s1 = printer.PrintToString(collections);
            s1.Should().Be(expected);
        }
        [Test]
        public void ObjectPrinter_SomeClassesInList_Object()
        {
            var collections = new Collections();
            collections.List = null;
            collections.Array = null;
            collections.Persons = new List<Person> {new Person(){Name = "Lev"} };
            var printer = ObjectPrinter.For<Collections>();
            const string expected = "Collections\r\n\tDictionary = \r\nnull\r\n\tArray = \r\nnull\r\n\tList = \r\nnull\r\n\t" +
                                    "Persons = \r\n\t\tPerson\r\n\t\tId = 00000000-0000-0000-0000-000000000000\r\n\t\tName = Lev\r\n\t\tHeight = 0\r\n\t\tAge = 0\r\n";
            var s1 = printer.PrintToString(collections);
            s1.Should().Be(expected);
        }
    }
}