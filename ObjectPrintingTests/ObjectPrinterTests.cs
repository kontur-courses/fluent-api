using System;
using System.Collections.Generic;
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
            person = new Person {Id = Guid.NewGuid(), Name = "Alex", Age = 19};
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
                .WithConfig(guid => $"Person id is {guid}");
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
                .WithConfig(guid => $"Person id is {guid}")
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
            var personWithWeight = new PersonWithWeight {Name = "Nik", Age = 54, Weight = 76.32};
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

        #region Loopback processing

        [Test]
        public void PrintObject_CorrectPrintNode_WhenHasSimpleLoopback()
        {
            var node1 = new Node();
            var node2 = new Node {PreviousNode = node1};
            node1.PreviousNode = node2;

            var result = node1.PrintToString(
                conf => conf.ExcludingProperty(node => node.NextNode));
            
            result.Should().Be(
                $"{node1.GetType().Name}{newLine}" +
                $"\tId = {node1.Id}{newLine}" +
                $"\tPreviousNode = {node1.PreviousNode.GetType().Name}{newLine}" +
                $"\t\tId = {node2.Id}{newLine}" +
                $"\t\tPreviousNode = Loopback detected{newLine}");
        }
        
        [Test]
        public void PrintObject_PrintWithoutLoopback()
        {
            var node1 = new Node();
            var node2 = new Node { PreviousNode = node1, NextNode = node1 };

            var result = node2.PrintToString();
            result.Should().NotContain("Loopback detected");
        }

        [Test]
        public void PrintObject_CorrectPrintNode_WhenHasDifficultLoopback()
        {
            var node1 = new Node {Id = 1};
            var node2 = new Node {Id = 2, PreviousNode = node1};
            var node3 = new Node {Id = 3, PreviousNode = node2};
            var node4 = new Node {Id = 4, PreviousNode = node3};
            node1.PreviousNode = node4;

            var result = node1.PrintToString(
                conf => conf.ExcludingProperty(node => node.NextNode));

            result.Should().Be(
                $"{node1.GetType().Name}{newLine}" +
                $"\tId = {node1.Id}{newLine}" +
                $"\tPreviousNode = {node1.PreviousNode.GetType().Name}{newLine}" +
                $"\t\tId = {node4.Id}{newLine}" +
                $"\t\tPreviousNode = {node4.PreviousNode.GetType().Name}{newLine}" +
                $"\t\t\tId = {node3.Id}{newLine}" +
                $"\t\t\tPreviousNode = {node3.PreviousNode.GetType().Name}{newLine}" +
                $"\t\t\t\tId = {node2.Id}{newLine}" +
                $"\t\t\t\tPreviousNode = Loopback detected{newLine}");
        }

        #endregion

        #region IEnumerable processing

        [Test]
        public void PrintObject_PrintEnumerableElements_WhenEnumerableIsDictionary()
        {
            var office = new Office
            {
                Employees = new Dictionary<int, Person>
                {
                    {1, new Person {Name = "Alex", Age = 21}},
                    {2, new Person {Name = "Klauz", Age = 26}}
                }
            };

            var result = office.PrintToString(conf =>
                conf.ExcludingPropertyWithType<Guid>()
                    .ExcludingProperty(office1 => office1.OfficeThings)
                    .ExcludingProperty(office1 => office1.Times));

            result.Should().Be(
                $"{office.GetType().Name}{newLine}" +
                $"\tEmployees = {office.Employees.GetType().Name}{newLine}" +
                $"\t\t1: Person{newLine}" +
                $"\t\t\tName = Alex{newLine}" +
                $"\t\t\tAge = 21{newLine}" +
                $"\t\t2: Person{newLine}" +
                $"\t\t\tName = Klauz{newLine}" +
                $"\t\t\tAge = 26{newLine}");
        }

        [Test]
        public void PrintObject_PrintEnumerableElements_WhenEnumerableIsArray()
        {
            var office = new Office
            {
                Times = new[]
                {
                    new DateTime(2020, 11, 26),
                    new DateTime(1999, 01, 22)
                }
            };

            var result = office.PrintToString(conf =>
                conf.ExcludingPropertyWithType<Guid>()
                    .ExcludingProperty(office1 => office1.OfficeThings)
                    .ExcludingProperty(office1 => office1.Employees));

            result.Should().Be(
                $"{office.GetType().Name}{newLine}" +
                $"\tTimes = {office.Times.GetType().Name}{newLine}" +
                $"\t\t0: 26.11.2020 0:00:00{newLine}" +
                $"\t\t1: 22.01.1999 0:00:00{newLine}");
        }

        [Test]
        public void PrintObject_PrintEnumerableElements_WhenEnumerableIsList()
        {
            var office = new Office
            {
                OfficeThings = new List<string>
                {
                    "Pen", "PC", "Table"
                }
            };

            var result = office.PrintToString(conf =>
                conf.ExcludingPropertyWithType<Guid>()
                    .ExcludingProperty(office1 => office1.Times)
                    .ExcludingProperty(office1 => office1.Employees));

            result.Should().Be(
                $"{office.GetType().Name}{newLine}" +
                $"\tOfficeThings = {office.OfficeThings.GetType().Name}{newLine}" +
                $"\t\t0: Pen{newLine}" +
                $"\t\t1: PC{newLine}" +
                $"\t\t2: Table{newLine}");
        }

        #endregion

        [Test]
        public void PrintObject_PrintPersonWithPersonProperty()
        {
            var person1 = new Person1 { Name = "Greetings", Friend = new Person { Name = "Hello" } };
            
            var printer = ObjectPrinter.For<Person1>()
                .PrintProperty(b => b.Friend.Name)
                .TrimmedToLength(2);
            var actual = printer.PrintToString(person1);
            
            actual.Should().Contain("Greetings");
        }
    }
}