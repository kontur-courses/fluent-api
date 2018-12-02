using System;
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
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Person");
            stringBuilder.AppendLine("\tId = Guid");
            stringBuilder.AppendLine("\tName = null");
            stringBuilder.AppendLine("\tHeight = 0");
            stringBuilder.AppendLine("\tAge = 0");
            
            var obj = new Person();
            var printer = ObjectPrinter.For<Person>();
            printer.PrintToString(obj).Should().Be(stringBuilder.ToString());
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
    }
}