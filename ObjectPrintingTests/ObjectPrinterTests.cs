using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrintingTests.TestsElements;

namespace ObjectPrinting
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        [Test]
        public void PrintToStringDefaultPerson()
        {
            var person = new Person();
            var printer = ObjectPrinter.For<Person>();
            
            var printingString = printer.PrintToString(person);

            foreach (var propertyInfo in typeof(Person).GetProperties())
                printingString.Should().Contain(propertyInfo.Name);
        }
        
        [Test]
        public void PrintToStringPersonWithAllExcludingTypes()
        {
            var person = new Person{ Name = "Alex", Age = 18};
            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>()
                .Excluding<int>()
                .Excluding<Guid>()
                .Excluding<double>()
                .Excluding<Person>();

            var printingString = printer.PrintToString(person);

            foreach (var propertyInfo in typeof(Person).GetProperties())
                printingString.Should().NotContain(propertyInfo.Name);
        }
        
        [Test]
        public void ExcludingClassTypes()
        {
            var person = new Person{ Name = "Alex", Age = 18};
            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>()
                .Excluding<Person>();

            var printingString = printer.PrintToString(person);

            printingString.Should().NotContain("Name");
            printingString.Should().NotContain("Parent");
        }
        
        [Test]
        public void ExcludingStructTypes()
        {
            var person = new Person{ Name = "Alex", Age = 18};
            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>()
                .Excluding<double>()
                .Excluding<Guid>();

            var printingString = printer.PrintToString(person);

            printingString.Should().NotContain("Age");
            printingString.Should().NotContain("Height");
            printingString.Should().NotContain("Id");
        }
        
        [Test]
        public void ExcludingClassProperty()
        {
            var person = new Person{ Name = "Alex", Age = 18};
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Parent);
               
            var printingString = printer.PrintToString(person);
               
            printingString.Should().NotContain("Parent");
        }
        
        [Test]
        public void ExcludingStructProperty()
        {
            var person = new Person{ Name = "Alex", Age = 18};
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Id);
               
            var printingString = printer.PrintToString(person);
               
            printingString.Should().NotContain("Id");
        }

        [Test]
        public void CustomSerializationForType()
        {
            var person = new Person{ Name = "Alex", Age = 18};
            var printer = ObjectPrinter.For<Person>()
                .Serialize<string>().Using(s => s.ToUpper());
            
            var printingString = printer.PrintToString(person);
            
            printingString.Should().Contain(person.Name.ToUpper());
        }
        
        [Test]
        public void CustomSerializationForProperty()
        {
            var person = new Person{ Name = "Alex", Age = 18};
            var printer = ObjectPrinter.For<Person>()
                .Serialize(p => p.Name).Using(s => $"Name of this = {s.ToUpper()}");
               
            var printingString = printer.PrintToString(person);

            printingString.Should().Contain($"Name of this = {person.Name.ToUpper()}");
        }

        [TestCase("ru-RU", 2.5, "2,5")]
        [TestCase("en-GB", 2.5, "2.5")]
        public void CultureForNumberTypes(string culture, double number, string expectedSerialiseNumber)
        {
            var person = new Person{ Name = "Alex", Age = 18, Height = number};
            var printer = ObjectPrinter.For<Person>()
                .Serialize<double>().WithCulture(new CultureInfo(culture));
                       
            var printingString = printer.PrintToString(person);

            printingString.Should().Contain(expectedSerialiseNumber);
        }
        
        [Test]
        public void CutForStringTypes()
        {
            var person = new Person{ Name = "Alex", Age = 18};
            var printer = ObjectPrinter.For<Person>()
                .Serialize(p => p.Name).Cut(3);
               
            var printingString = printer.PrintToString(person);

            printingString.Should().Contain("Ale");
        }
    }
}