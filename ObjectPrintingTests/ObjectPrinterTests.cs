using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.ObjectExtensions;
using ObjectPrinting.PropertyPrintingConfig;
using ObjectPrintingTests.TestModels;

namespace ObjectPrintingTests
{ 
    public class ObjectPrinterTests
    {
        [SetUp]
        public void SetUp()
        {
            person = new Person { Id = Guid.NewGuid(), Name = "Alex", Age = 19};
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
                $"\tName = String data: {person.Name}{newLine}" +
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
                $"\tId = Person id is {person.Id.ToString()}{newLine}" +
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
                $"\tId = Person id is {person.Id}{newLine}" +
                $"\tName = Person name is {person.Name}{newLine}" +
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
                $"\tId = Nice guid - {person.Id}{newLine}" +
                $"\tName = Person name is {person.Name}{newLine}" +
                $"\tAge = {person.Age}{newLine}");
        }
        
        #endregion

        #region Serizlization method with specified culure for type and property

        [Test]
        public void PrintObject_PrintPersonWithSpecialCultureForType()
        {
            var personWithWeight = new PersonWithWeight{Name = "Nik", Age = 54, Weight = 76.32};
            var printer = ObjectPrinter.For<PersonWithWeight>()
                .PrintProperty<double>()
                .WithCulture(CultureInfo.CurrentCulture);
            var result = printer.PrintToString(personWithWeight);
            
            result.Should().Be(
                $"{personWithWeight.GetType().Name}{newLine}" +
                $"\tId = {personWithWeight.Id}{newLine}" +
                $"\tName = {personWithWeight.Name}{newLine}" +
                $"\tAge = {personWithWeight.Age}{newLine}" +
                $"\tWeight = {personWithWeight.Weight.ToString(null, CultureInfo.CurrentCulture)}{newLine}");
        }

        #endregion

        #region Serializaton method that trims string properties

        [Test]
        public void PrintObject_PrintPersonWithTrimsStringTypes()
        {
            var printer = ObjectPrinter.For<Person>()
                .PrintProperty<string>()
                .TrimmedToLength(2);
            var result = printer.PrintToString(person);

            result.Should().Be(
                $"{person.GetType().Name}{newLine}" +
                $"\tId = {person.Id}{newLine}" +
                $"\tName = {person.Name.Substring(0, 2)}{newLine}" +
                $"\tAge = {person.Age}{newLine}");
        }
        
        [Test]
        public void PrintObject_PrintPersonWithTrimsNameProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .PrintProperty(pers => pers.Name)
                .TrimmedToLength(2);
            var result = printer.PrintToString(person);

            result.Should().Be(
                $"{person.GetType().Name}{newLine}" +
                $"\tId = {person.Id}{newLine}" +
                $"\tName = {person.Name.Substring(0, 2)}{newLine}" +
                $"\tAge = {person.Age}{newLine}");
        }

        #endregion

        #region Default and configured serialization extension methods

        [Test]
        public void PrintObject_PrintPersonWithDefaultSerialization()
        {
            var result = person.PrintToString();

            result.Should().Be(
                $"{person.GetType().Name}{newLine}" +
                $"\tId = {person.Id}{newLine}" +
                $"\tName = {person.Name}{newLine}" +
                $"\tAge = {person.Age}{newLine}");
        }
        
        [Test]
        public void PrintObject_PrintPersonWithConfiguredSerialization()
        {
            var result = person.PrintToString(config => config
                .ExcludingProperty(pers => pers.Age)
                .PrintProperty(pers => pers.Name)
                .WithConfig(name => name.ToUpper()));

            result.Should().Be(
                $"{person.GetType().Name}{newLine}" +
                $"\tId = {person.Id}{newLine}" +
                $"\tName = {person.Name.ToUpper()}{newLine}");
        }

        #endregion
    }
}