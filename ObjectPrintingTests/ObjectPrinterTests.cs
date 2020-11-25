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

        #region Alternative serialization method for specified type and property

        [Test]
        public void PrintObject_PrintPersonWithAlternativeSerializationMethodForType()
        {
            var printer = ObjectPrinter.For<Person>()
                .PrintProperty<string>()
                .WithConfig(str => $"String data: {str}");
            var result = printer.PrintToString(person);
            
            result.Should().Be(
                $"Person{newLine}" +
                $"\tId = {person.Id}{newLine}" +
                $"\tString data: {person.Name}{newLine}" +
                $"\tAge = {person.Age}{newLine}");
        }
        
        [Test]
        public void PrintObject_PrintPersonWithAlternativeSerializationMethodForProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .PrintProperty(pers => pers.Id)
                .WithConfig(guid => $"Person id is {guid.ToString()}");
            var result = printer.PrintToString(person);
            
            result.Should().Be(
                $"Person{newLine}" +
                $"\tPerson id is {person.Id.ToString()}{newLine}" +
                $"\tName = {person.Name}{newLine}" +
                $"\tAge = {person.Age}{newLine}");
        }
        
        [Test]
        public void PrintObject_PrintPersonWithAlternativeSerializationMethod_ForTwoProperties()
        {
            var printer = ObjectPrinter.For<Person>()
                .PrintProperty(pers => pers.Id)
                .WithConfig(guid => $"Person id is {guid.ToString()}")
                .PrintProperty(pers => pers.Name)
                .WithConfig(str => $"Person name is {str}");
            var result = printer.PrintToString(person);

            result.Should().Be(
                $"Person{newLine}" +
                $"\tPerson id is {person.Id}{newLine}" +
                $"\tPerson name is {person.Name}{newLine}" +
                $"\tAge = {person.Age}{newLine}");
        }

        [Test]
        public void PrintObject_PrintPersonWithAlternativeSerializationMethodForTypeAndProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .PrintProperty<Guid>()
                .WithConfig(guid => $"Nice guid - {guid}")
                .PrintProperty(pers => pers.Name)
                .WithConfig(name => $"Person name is {name}");
            var result = printer.PrintToString(person);
            
            result.Should().Be(
                $"Person{newLine}" +
                $"\tNice guid - {person.Id}{newLine}" +
                $"\tPerson name is {person.Name}{newLine}" +
                $"\tAge = {person.Age}{newLine}");
        }
        
        #endregion
    }
}