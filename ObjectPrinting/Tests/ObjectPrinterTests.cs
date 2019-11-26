using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    internal class ObjectPrinterTests
    {
        private Person _person;
        private Person _parent;
        private string _result;

        [SetUp]
        public void SetUp()
        {
            _parent = new Person {Name = "Darth", Age = 1000, Id = Guid.Empty, Height = 0};
            _person = new Person {Name = "Luke", Age = 20, Id = Guid.Empty, Height = 150.5, Parent = _parent};
            _result = string.Format("Person{0}" +
                                    "\tId = 00000000-0000-0000-0000-000000000000{0}" +
                                    "\tName = Luke{0}" +
                                    "\tHeight = 150,5{0}" +
                                    "\tAge = 20{0}" +
                                    "\tParent = Person{0}" +
                                    "\t\tId = 00000000-0000-0000-0000-000000000000{0}" +
                                    "\t\tName = Darth{0}" +
                                    "\t\tHeight = 0{0}" +
                                    "\t\tAge = 1000{0}" +
                                    "\t\tParent = null{0}", Environment.NewLine);
        }

        [Test]
        public void PrintToString_WorksCorrectly_WithNestedObject()
        {
            var printer = ObjectPrinter.For<Person>();
            var result = printer.PrintToString(_person);
            result.Should().Be(_result);
        }

        [Test]
        public void PrintToString_WorksCorrectly_OnList()
        {
            var list = new List<int> {0, 1, -1};
            var printer = ObjectPrinter.For<List<int>>();
            var result = printer.PrintToString(list);
            result.Should().Be(String.Format(
                "List`1{0}" +
                "\t0{0}" +
                "\t1{0}" +
                "\t-1{0}", Environment.NewLine));
        }

        [Test]
        public void PrintToString_WorksCorrectly_OnDictionary()
        {
            var dict = new Dictionary<int, string> {{1, "one"}, {-1, "negative one"}};
            var printer = ObjectPrinter.For<Dictionary<int, string>>();
            var result = printer.PrintToString(dict);
            result.Should().Be(String.Format(
                "Dictionary`2{0}" +
                "\tKeyValuePair`2{0}" +
                "\t\tKey = 1{0}" +
                "\t\tValue = one{0}" +
                "\tKeyValuePair`2{0}" +
                "\t\tKey = -1{0}" +
                "\t\tValue = negative one{0}", Environment.NewLine));
        }

        [Test]
        public void PrintToString_WorksCorrectly_OnObjectWithListProperty()
        {
            var salaries = new List<int> {2000, 3000, 2500};
            var student = new Worker {FirstName = "A", SecondName = "B", Salaries = salaries};
            var printer = ObjectPrinter.For<Worker>();
            var result = printer.PrintToString(student);
            result.Should().Be(String.Format(
                "Worker{0}" +
                "\tFirstName = A{0}" +
                "\tSecondName = B{0}" +
                "\tSalaries = List`1{0}" +
                "\t\t2000{0}" +
                "\t\t3000{0}" +
                "\t\t2500{0}", Environment.NewLine));
        }

        [Test]
        public void PrintToString_DoesNotThrowException_OnObjectWithCycleReference()
        {
            _parent.Parent = _person;

            var printer = ObjectPrinter.For<Person>();
            Action act = () => printer.PrintToString(_person);
            act.ShouldNotThrow();
        }

        [Test]
        public void Excluding_WorksCorrectly_OnComplicatedObject()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Person>();
            var result = printer.PrintToString(_person);
            result.Should().Be((_result.Substring(0, result.Length)));
        }

        [Test]
        public void Excluding_ExcludesType_InNestedObjects()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>();
            var result = printer.PrintToString(_person);
            result.Should().Be(_result
                .Replace($"\t\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}", "")
                .Replace($"\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}", ""));
        }

        [Test]
        public void Excluding_ExcludesCorrectly_OnObjectWithListProperty()
        {
            var marks = new List<int> {2, 3, 2, 4};
            var student = new Worker {FirstName = "A", SecondName = "B", Salaries = marks};
            var printer = ObjectPrinter.For<Worker>()
                .Excluding<int>();
            var result = printer.PrintToString(student);
            result.Should().Be(String.Format(
                "Worker{0}" +
                "\tFirstName = A{0}" +
                "\tSecondName = B{0}" +
                "\tSalaries = List`1{0}", Environment.NewLine));
        }

        [Test]
        public void TypePrinting_WorksCorrectly_OnComplicatedObject()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(i => "X");
            var result = printer.PrintToString(_person);
            result.Should().Be(_result
                .Replace("20", "X")
                .Replace("1000", "X"));
        }

        [Test]
        public void TypePrinting_WorksCorrectly_OnList()
        {
            var list = new List<Person> {new Person {Name = "First"}, new Person {Name = "Second"}};
            var printer = ObjectPrinter.For<List<Person>>()
                .Printing<Person>().Using(i => "X");
            var result = printer.PrintToString(list);
            result.Should().Be(String.Format(
                "List`1{0}" +
                "\tX{0}" +
                "\tX{0}", Environment.NewLine));
        }

        [Test]
        public void Culture_WorksCorrectly_InNestedObject()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(new CultureInfo("en-US"));
            var result = printer.PrintToString(_person);
            result.Should().Be(_result.Replace(",", "."));
        }


        [Test]
        public void Culture_WorksCorrectly_OnList()
        {
            var list = new List<double> {1.1, 2.2, 3.3};
            var printer = ObjectPrinter.For<List<double>>()
                .Printing<double>().Using(new CultureInfo("en-US"));
            var result = printer.PrintToString(list);
            result.Should().Be(string.Format(
                "List`1{0}" +
                "\t1.1{0}" +
                "\t2.2{0}" +
                "\t3.3{0}", Environment.NewLine));
        }


        [Test]
        public void PropertyPrinting_WorksCorrectly_OnComplicatedObjects()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Age).Using(i => "Well, " + i);
            var result = printer.PrintToString(_person);
            result.Should().Be(_result
                .Replace("20", "Well, 20"));
        }

        [Test]
        public void TrimMethod_WorksCorrectly_OnComplicatedObjects()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(2);
            var result = printer.PrintToString(_person);
            result.Should().Be(_result
                .Replace("Luke", "Lu"));
        }

        [Test]
        public void ObjectExtensionPrintToString_WorksCorrectly_OnComplicatedObject()
        {
            var result = _person.PrintToString();
            result.Should().Be(_result);
        }

        [Test]
        public void PrintToString_WithConfigurations_WorksCorrectly()
        {
            var result = _person.PrintToString(s => s.Excluding(p => p.Age));
            result.Should().Be(_result
                .Replace($"\tAge = 20{Environment.NewLine}", ""));
        }

        [Test]
        public void PrintToString_WorksCorrectly_WithCombinationOfMethods1()
        {
            var father = new Person {Name = "D", Age = 22, Id = Guid.Empty, Height = 100.5};
            var person = new Person {Name = "N", Age = 33, Id = Guid.Empty, Height = 1.5, Parent = father};
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing<double>().Using(new CultureInfo("en-US"))
                .Printing<int>().Using(i => i + " y")
                .Printing<string>().Using(i => i + " No-no");
            var result = printer.PrintToString(person);
            result.Should().Be(String.Format(
                "Person{0}" +
                "\tName = N No-no{0}" +
                "\tHeight = 1.5{0}" +
                "\tAge = 33 y{0}" +
                "\tParent = Person{0}" +
                "\t\tName = D No-no{0}" +
                "\t\tHeight = 100.5{0}" +
                "\t\tAge = 22 y{0}" +
                "\t\tParent = null{0}", Environment.NewLine));
        }


        private static IEnumerable<int> InfinityCollection()
        {
            var count = 0;
            while (true)
            {
                yield return count++;
            }
        }

        [Test, Timeout(1000)]
        public void PrintToString_DoesNotWorkInfinitely_OnInfiniteIEnumerable()
        {
            var printer = ObjectPrinter.For<IEnumerable<int>>();
            printer.PrintToString(InfinityCollection());
        }
    }
}