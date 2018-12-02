using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        [Test]
        public void ObjectPrinter_ShouldPrintCorrectString()
        {
            var expected = new StringBuilder();
            expected.AppendLine("Person");
            expected.AppendLine("\tId = Guid");
            expected.AppendLine("\tName = null");
            expected.AppendLine("\tHeight = 0");
            expected.AppendLine("\tAge = 0");
            expected.AppendLine("\tChild = null");
            
            var obj = new Person();
            var printer = ObjectPrinter.For<Person>();
            printer.PrintToString(obj).Should().Be(expected.ToString());
        }
        
        [Test]
        public void ObjectPrinter_ShouldPrintCorrectString_WhenExcludedDouble()
        {
            var obj = new Person() { Height = 1.8 };
            var printer = ObjectPrinter.For<Person>().Exclude<double>();
            printer.PrintToString(obj).Should().NotContain("Height = 1.8");
        }
        
        [Test]
        public void ObjectPrinter_ShouldPrintCorrectString_WhenExcludedInt()
        {
            var obj = new Person() { Age = 23 };
            var printer = ObjectPrinter.For<Person>().Exclude<int>();
            printer.PrintToString(obj).Should().NotContain("Age = 23");
        }
        
        [Test]
        public void ObjectPrinter_ShouldPrintCorrectString_WhenExcludedString()
        {
            var obj = new Person() { Name = "name" };
            var printer = ObjectPrinter.For<Person>().Exclude<string>();
            printer.PrintToString(obj).Should().NotContain("Name = name");
        }
        
        [Test]
        public void ObjectPrinter_ShouldPrintCorrectString_WhenExcludedCustomType()
        {
            var obj = new Person() { Id = Guid.Empty };
            var printer = ObjectPrinter.For<Person>().Exclude<Guid>();
            printer.PrintToString(obj).Should().NotContain("Id = Guid");
        }
        
        [Test]
        public void ObjectPrinter_ShouldPrintCorrectString_WhenExcludedProperty()
        {
            var obj = new Person() { Id = Guid.Empty };
            var printer = ObjectPrinter.For<Person>().Exclude(p => p.Id);
            printer.PrintToString(obj).Should().NotContain("Id = Guid");
        }
        
        [Test]
        public void ObjectPrinter_ShouldPrintCorrectString_WhenUsingSerializeFnForDouble()
        {
            var obj = new Person() { Height = 1.8 };
            var printer = ObjectPrinter.For<Person>().Serialize<double>().Using(p => "custom");
            printer.PrintToString(obj).Should().Contain("Height = custom");
        }
        
        [Test]
        public void ObjectPrinter_ShouldPrintCorrectString_WhenUsingSerializeFnForInt()
        {
            var obj = new Person() { Age = 23 };
            var printer = ObjectPrinter.For<Person>().Serialize<int>().Using(p => "custom");
            printer.PrintToString(obj).Should().Contain("Age = custom");
        }
        
        [Test]
        public void ObjectPrinter_ShouldPrintCorrectString_WhenUsingSerializeFnForString()
        {
            var obj = new Person() { Name = "name" };
            var printer = ObjectPrinter.For<Person>().Serialize<string>().Using(p => "custom");
            printer.PrintToString(obj).Should().Contain("Name = custom");
        }
        
        [Test]
        public void ObjectPrinter_ShouldPrintCorrectString_WhenUsingSerializeFnForCustomType()
        {
            var obj = new Person() { Id = Guid.Empty };
            var printer = ObjectPrinter.For<Person>().Serialize<Guid>().Using(p => p.ToString());
            printer.PrintToString(obj).Should().Contain($"Id = {Guid.Empty.ToString()}");
        }
        
        [Test]
        public void ObjectPrinter_ShouldPrintCorrectString_WhenUsingSerializeNumbersByCulture()
        {
            var cultureInfo = new CultureInfo("fr-FR");
            var obj = new Person() { Height = 1.8 };
            var printer = ObjectPrinter.For<Person>().Serialize<double>().Using(cultureInfo);
            printer.PrintToString(obj).Should().Contain($"Height = {obj.Height.ToString(cultureInfo)}");
        }
        
        [Test]
        public void ObjectPrinter_ShouldPrintCorrectString_WhenUsingSerializeFnForProperty()
        {
            var obj = new Person() { Id = Guid.Empty };
            var printer = ObjectPrinter.For<Person>().Serialize(p => p.Id).Using(p => "custom");
            printer.PrintToString(obj).Should().Contain("Id = custom");
        }
        
        [Test]
        public void ObjectPrinter_ShouldPrintCorrectString_WhenUsingCutForString()
        {
            var obj = new Person() { Name = "long name" };
            var printer = ObjectPrinter.For<Person>().Serialize<string>().Cut(4);
            printer.PrintToString(obj).Should().Contain("Name = long");
        }
        
        [Test]
        public void ObjectPrinter_ShouldPrintCorrectString_WhenObjectContainCycleLinks()
        {
            var p1 = new Person() { Name = "Person 1" };
            var p2 = new Person() { Name = "Person 2" };

            p1.Child = p2;
            p2.Child = p1;
            
            var expected = new StringBuilder();
            expected.AppendLine("Person");
            expected.AppendLine("\tId = Guid");
            expected.AppendLine("\tName = Person 1");
            expected.AppendLine("\tHeight = 0");
            expected.AppendLine("\tAge = 0");
            expected.AppendLine("\tChild = Person");
            expected.AppendLine("\t\tId = Guid");
            expected.AppendLine("\t\tName = Person 2");
            expected.AppendLine("\t\tHeight = 0");
            expected.AppendLine("\t\tAge = 0");
            expected.AppendLine("\t\tChild = Person");
            
            var printer = ObjectPrinter.For<Person>(1);
            printer.PrintToString(p1).Should().Be(expected.ToString());
        }

        [Test]
        public void ObjectPrinter_ShouldPrintCorrectString_WhenObjectIsList()
        {
            var personsList = new List<Person>()
            {
                new Person() { Name = "Person 1" },
                new Person() { Name = "Person 2" },
            };
            
            var expected = new StringBuilder();
            expected.AppendLine("List");
            expected.AppendLine("\t[0] = Person");
            expected.AppendLine("\t\tId = Guid");
            expected.AppendLine("\t\tName = Person 1");
            expected.AppendLine("\t\tHeight = 0");
            expected.AppendLine("\t\tAge = 0");
            expected.AppendLine("\t\tChild = null");
            expected.AppendLine("\t[1] = Person");
            expected.AppendLine("\t\tId = Guid");
            expected.AppendLine("\t\tName = Person 2");
            expected.AppendLine("\t\tHeight = 0");
            expected.AppendLine("\t\tAge = 0");
            expected.AppendLine("\t\tChild = null");
            
            var printer = ObjectPrinter.For<List<Person>>();
            printer.PrintToString(personsList).Should().Be(expected.ToString());
        }

        [Test]
        public void ObjectPrinter_ShouldPrintCorrectString_WhenObjectIsArray()
        {
            var personsList = new Person[]
            {
                new Person() { Name = "Person 1" },
                new Person() { Name = "Person 2" },
            };
            
            var expected = new StringBuilder();
            expected.AppendLine("Array");
            expected.AppendLine("\t[0] = Person");
            expected.AppendLine("\t\tId = Guid");
            expected.AppendLine("\t\tName = Person 1");
            expected.AppendLine("\t\tHeight = 0");
            expected.AppendLine("\t\tAge = 0");
            expected.AppendLine("\t\tChild = null");
            expected.AppendLine("\t[1] = Person");
            expected.AppendLine("\t\tId = Guid");
            expected.AppendLine("\t\tName = Person 2");
            expected.AppendLine("\t\tHeight = 0");
            expected.AppendLine("\t\tAge = 0");
            expected.AppendLine("\t\tChild = null");
            
            var printer = ObjectPrinter.For<Person[]>();
            printer.PrintToString(personsList).Should().Be(expected.ToString());
        }

        [Test]
        public void ObjectPrinter_ShouldPrintCorrectString_WhenObjectIsDictionary()
        {
            var personsList = new Dictionary<string, Person>()
            {
                {"p1", new Person() { Name = "Person 1" }},
                {"p2", new Person() { Name = "Person 2" }},
            };
            
            var expected = new StringBuilder();
            expected.AppendLine("Dictionary");
            expected.AppendLine("\t[`p1`] = Person");
            expected.AppendLine("\t\tId = Guid");
            expected.AppendLine("\t\tName = Person 1");
            expected.AppendLine("\t\tHeight = 0");
            expected.AppendLine("\t\tAge = 0");
            expected.AppendLine("\t\tChild = null");
            expected.AppendLine("\t[`p2`] = Person");
            expected.AppendLine("\t\tId = Guid");
            expected.AppendLine("\t\tName = Person 2");
            expected.AppendLine("\t\tHeight = 0");
            expected.AppendLine("\t\tAge = 0");
            expected.AppendLine("\t\tChild = null");
            
            var printer = ObjectPrinter.For<Dictionary<string, Person>>();
            printer.PrintToString(personsList).Should().Be(expected.ToString());
        }
    }
}