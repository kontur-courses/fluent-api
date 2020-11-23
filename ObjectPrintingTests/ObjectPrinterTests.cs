using System;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrintingTests.TestModels;

namespace ObjectPrintingTests
{ 
    public class ObjectPrinterTests
    {
        [SetUp]
        public void SetUp()
        {
            person = new Person { Id = Guid.NewGuid(), Name = "Alex", Age = 19 };
            newLine = Environment.NewLine;
        }

        private Person person;
        private string newLine;
        
        #region Excluding property with specified types and properties
        
        [Test]
        public void PrintObject_PrintOnlyObjectType_WhenExcludedAllTypes()
        {
            var printer = ObjectPrinter.For<Person>()
                .ExcludingPropertyWithTypes(typeof(int), typeof(string), typeof(Guid));
            var result = printer.PrintToString(person);

            result.Should().Be($"Person{newLine}");
        }

        [Test]
        public void PrintObject_PrintPersonWithoutOneType_WhenExcludedOneType()
        {
            var printer = ObjectPrinter.For<Person>()
                .ExcludingPropertyWithType<Guid>();
            var result = printer.PrintToString(person);
            
            result.Should().Be(
                $"Person{newLine}" +
                $"\tName = {person.Name}{newLine}" +
                $"\tAge = {person.Age}{newLine}");
        }
        
        [Test]
        public void PrintObject_PrintPersonWithoutOneProperty_WhenExcludedOneProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .ExcludingProperty(pers => pers.Id);
            var result = printer.PrintToString(person);
            
            result.Should().Be(
                $"Person{newLine}" +
                $"\tName = {person.Name}{newLine}" +
                $"\tAge = {person.Age}{newLine}");
        }
        
        #endregion
    }
}